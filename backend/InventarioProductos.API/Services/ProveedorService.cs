using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class ProveedorService : IProveedorService
{
    private readonly InventarioContext _context;
    private readonly IAuditoriaService _auditoria;
    private readonly IUsuarioActual _usuarioActual;
    private readonly ILogger<ProveedorService> _logger;

    public ProveedorService(InventarioContext context, IAuditoriaService auditoria,
        IUsuarioActual usuarioActual, ILogger<ProveedorService> logger)
    {
        _context = context;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _logger = logger;
    }

    public async Task<List<ProveedorDto>> ListarAsync(FiltroEstado estado = FiltroEstado.Activos, string? buscar = null)
    {
        var query = _context.Proveedores.AsNoTracking();
        query = estado switch
        {
            FiltroEstado.Inactivos => query.Where(p => !p.Estado),
            FiltroEstado.Todos => query,
            _ => query.Where(p => p.Estado)
        };

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            query = query.Where(p => p.Nombre.Contains(texto) || p.Email.Contains(texto));
        }

        return await Proyectar(query).OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<ProveedorDto> ReactivarAsync(int id)
    {
        var proveedor = await _context.Proveedores
            .Include(p => p.Lotes).ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NoEncontradoException("El proveedor no existe.");

        if (proveedor.Estado)
            throw new ReglaNegocioException("El proveedor ya está activo.");

        // Reactivación en cascada: vuelven todos sus lotes y los productos a los que pertenecen.
        proveedor.Estado = true;
        foreach (var lote in proveedor.Lotes)
        {
            lote.Estado = true;
            lote.Producto.Estado = true;
        }
        _auditoria.Registrar("Reactivar", "Proveedor", id,
            $"{proveedor.Nombre} reactivado junto con sus {proveedor.Lotes.Count} lotes y productos");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} reactivó el proveedor {IdProveedor} ({Nombre})",
            _usuarioActual.Nombre, id, proveedor.Nombre);

        return await ObtenerAsync(id);
    }

    public async Task<ProveedorDto> CrearAsync(ProveedorGuardarDto dto)
    {
        var nombre = dto.Nombre.Trim();

        if (await _context.Proveedores.AnyAsync(p => p.Nombre == nombre))
            throw new ReglaNegocioException("Ya existe un proveedor con ese nombre.");

        var proveedor = new Proveedor
        {
            Nombre = nombre,
            Email = dto.Email.Trim().ToLower(),
            Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : dto.Celular.Trim(),
            Estado = true
        };

        // El id se conoce después del primer guardado: transacción para que el proveedor y su auditoría
        // queden juntos o no quede ninguno.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        _auditoria.Registrar("Crear", "Proveedor", proveedor.Id, $"{proveedor.Nombre} ({proveedor.Email})");
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();

        _logger.LogInformation("Auditoría: {Usuario} creó el proveedor {IdProveedor} ({Nombre})",
            _usuarioActual.Nombre, proveedor.Id, proveedor.Nombre);

        return await ObtenerAsync(proveedor.Id);
    }

    public async Task<ProveedorDto> ActualizarAsync(int id, ProveedorGuardarDto dto)
    {
        var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El proveedor no existe.");

        var nombre = dto.Nombre.Trim();

        if (await _context.Proveedores.AnyAsync(p => p.Nombre == nombre && p.Id != id))
            throw new ReglaNegocioException("Ya existe un proveedor con ese nombre.");

        var email = dto.Email.Trim().ToLower();
        var celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : dto.Celular.Trim();

        // Detalle de auditoría con el antes y el después de cada campo que cambió.
        var cambios = new List<string>();
        if (proveedor.Nombre != nombre) cambios.Add($"nombre {proveedor.Nombre} -> {nombre}");
        if (proveedor.Email != email) cambios.Add($"correo {proveedor.Email} -> {email}");
        if (proveedor.Celular != celular) cambios.Add($"celular {proveedor.Celular ?? "-"} -> {celular ?? "-"}");

        proveedor.Nombre = nombre;
        proveedor.Email = email;
        proveedor.Celular = celular;

        var detalle = cambios.Count > 0 ? string.Join("; ", cambios) : "sin cambios";
        _auditoria.Registrar("Actualizar", "Proveedor", id, $"{nombre}: {detalle}");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} actualizó el proveedor {IdProveedor}: {Detalle}",
            _usuarioActual.Nombre, id, detalle);

        return await ObtenerAsync(id);
    }

    public async Task DesactivarAsync(int id)
    {
        var proveedor = await _context.Proveedores
            .Include(p => p.Lotes)
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El proveedor no existe.");

        // Baja en cascada: al desactivar el proveedor también se desactivan todos sus lotes.
        proveedor.Estado = false;
        foreach (var lote in proveedor.Lotes)
            lote.Estado = false;
        var lotes = proveedor.Lotes.Count;
        _auditoria.Registrar("Eliminar", "Proveedor", id, $"{proveedor.Nombre} desactivado junto con sus {lotes} lotes");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} desactivó el proveedor {IdProveedor} ({Nombre}) y sus {Lotes} lotes",
            _usuarioActual.Nombre, id, proveedor.Nombre, lotes);
    }

    private async Task<ProveedorDto> ObtenerAsync(int id) =>
        await Proyectar(_context.Proveedores.AsNoTracking().Where(p => p.Id == id)).FirstAsync();

    private static IQueryable<ProveedorDto> Proyectar(IQueryable<Proveedor> query) =>
        query.Select(p => new ProveedorDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Email = p.Email,
            Celular = p.Celular,
            Estado = p.Estado,
            LotesActivos = p.Lotes.Count(l => l.Estado && l.Producto.Estado)
        });
}
