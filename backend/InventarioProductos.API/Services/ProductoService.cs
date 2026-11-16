using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class ProductoService : IProductoService
{
    private readonly InventarioContext _context;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IAuditoriaService _auditoria;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(InventarioContext context, IUsuarioActual usuarioActual,
        IAuditoriaService auditoria, ILogger<ProductoService> logger)
    {
        _context = context;
        _usuarioActual = usuarioActual;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PaginadoDto<ProductoListadoDto>> ListarAsync(string? buscar, int pagina, int tamanoPagina, FiltroEstado estado = FiltroEstado.Activos)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var query = _context.Productos.AsNoTracking();
        query = estado switch
        {
            FiltroEstado.Inactivos => query.Where(p => !p.Estado),
            FiltroEstado.Todos => query,
            _ => query.Where(p => p.Estado)
        };

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(p => p.Nombre.Contains(buscar) || p.Codigo.Contains(buscar));

        var total = await query.CountAsync();

        var items = await Proyectar(query.OrderBy(p => p.Id).Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina))
            .ToListAsync();

        return new PaginadoDto<ProductoListadoDto>
        {
            Items = items,
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    public async Task<ProductoListadoDto> ObtenerAsync(int id)
    {
        var producto = await Proyectar(_context.Productos.AsNoTracking().Where(p => p.Id == id && p.Estado))
            .FirstOrDefaultAsync();

        return producto ?? throw new NoEncontradoException("El producto no existe.");
    }

    public async Task<ProductoListadoDto> CrearAsync(ProductoGuardarDto dto)
    {
        var codigo = dto.Codigo.Trim().ToUpper();

        if (await _context.Productos.AnyAsync(p => p.Codigo == codigo))
            throw new ReglaNegocioException("Ya existe un producto con ese código.");

        await ValidarOfertasAsync(dto.Ofertas);

        var producto = new Producto
        {
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Estado = true,
            Lotes = dto.Ofertas.Select(o => new Lote
            {
                IdProveedor = o.IdProveedor,
                NumeroLote = o.NumeroLote.Trim().ToUpper(),
                Estado = true,
                Inventario = new Inventario { Costo = o.Costo, Precio = o.Precio, Stock = o.Stock }
            }).ToList()
        };

        // El id del producto se conoce después del primer guardado, por eso se usa una
        // transacción: si falla la auditoría, tampoco queda creado el producto.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        _auditoria.Registrar("Crear", "Producto", producto.Id,
            $"Producto {producto.Codigo} - {producto.Nombre} con {producto.Lotes.Count} lote(s)");
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();

        _logger.LogInformation(
            "Auditoría: {Usuario} creó el producto {IdProducto} ({Codigo}) con {CantidadOfertas} lote(s)",
            _usuarioActual.Nombre, producto.Id, producto.Codigo, producto.Lotes.Count);

        return await ObtenerAsync(producto.Id);
    }

    // Solo los datos del catálogo. Los lotes se agregan, desactivan o ajustan con sus propias operaciones.
    public async Task<ProductoListadoDto> ActualizarAsync(int id, ProductoActualizarDto dto)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El producto no existe.");

        var codigo = dto.Codigo.Trim().ToUpper();

        if (await _context.Productos.AnyAsync(p => p.Codigo == codigo && p.Id != id))
            throw new ReglaNegocioException("Ya existe un producto con ese código.");

        // Detalle de auditoría con el antes y el después de cada campo que cambió.
        var cambios = new List<string>();
        if (producto.Codigo != codigo) cambios.Add($"código {producto.Codigo} -> {codigo}");
        if (producto.Nombre != dto.Nombre.Trim()) cambios.Add($"nombre {producto.Nombre} -> {dto.Nombre.Trim()}");
        if ((producto.Descripcion ?? "") != (dto.Descripcion?.Trim() ?? "")) cambios.Add("descripción actualizada");

        producto.Codigo = codigo;
        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion?.Trim();

        var detalle = cambios.Count > 0 ? string.Join("; ", cambios) : "sin cambios";
        _auditoria.Registrar("Actualizar", "Producto", id, $"Producto {codigo}: {detalle}");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} actualizó el producto {IdProducto} ({Codigo}): {Detalle}",
            _usuarioActual.Nombre, id, codigo, detalle);

        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Lotes.Where(l => l.Estado))
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El producto no existe.");

        // Baja en cascada: al desactivar el producto también se desactivan todos sus lotes.
        producto.Estado = false;
        foreach (var lote in producto.Lotes)
            lote.Estado = false;

        _auditoria.Registrar("Eliminar", "Producto", id,
            $"Producto {producto.Codigo} - {producto.Nombre} desactivado junto con sus {producto.Lotes.Count} lotes");
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Auditoría: {Usuario} eliminó el producto {IdProducto} ({Codigo})",
            _usuarioActual.Nombre, id, producto.Codigo);
    }

    public async Task<MatrizPreciosDto> ObtenerMatrizPreciosAsync(string? buscar, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var proveedores = await _context.Proveedores
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorDto { Id = p.Id, Nombre = p.Nombre })
            .ToListAsync();

        var query = _context.Productos.AsNoTracking().Where(p => p.Estado);

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(p => p.Nombre.Contains(buscar) || p.Codigo.Contains(buscar));

        var total = await query.CountAsync();

        // SQL trae solo la página con los precios de sus lotes activos; la agrupación
        // por proveedor se hace en memoria porque son pocas filas.
        var productos = await query
            .OrderBy(p => p.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                Ofertas = p.Lotes
                    .Where(l => l.Estado && l.Inventario != null)
                    .Select(l => new { l.IdProveedor, l.Inventario!.Precio, l.Inventario.Stock })
                    .ToList()
            })
            .ToListAsync();

        return new MatrizPreciosDto
        {
            Proveedores = proveedores,
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            Items = productos.Select(p => new FilaMatrizPreciosDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                // Varios lotes del mismo proveedor: se muestra el precio menor.
                // Por proveedor: el precio menor entre los lotes con stock (lo que de verdad se puede comprar);
                // si ninguno tiene stock, el menor de todos. El stock es la suma de sus lotes.
                Precios = p.Ofertas
                    .GroupBy(o => o.IdProveedor)
                    .ToDictionary(g => g.Key, g => new PrecioProveedorDto
                    {
                        Precio = g.Any(o => o.Stock > 0) ? g.Where(o => o.Stock > 0).Min(o => o.Precio) : g.Min(o => o.Precio),
                        Stock = g.Sum(o => o.Stock)
                    })
            }).ToList()
        };
    }

    private async Task ValidarOfertasAsync(List<OfertaGuardarDto> ofertas)
    {
        var lotes = ofertas.Select(o => o.NumeroLote.Trim().ToUpper()).ToList();

        if (lotes.Distinct().Count() != lotes.Count)
            throw new ReglaNegocioException("Hay números de lote repetidos en la solicitud.");

        var idsProveedor = ofertas.Select(o => o.IdProveedor).Distinct().ToList();
        var existentes = await _context.Proveedores.CountAsync(p => idsProveedor.Contains(p.Id) && p.Estado);

        if (existentes != idsProveedor.Count)
            throw new ReglaNegocioException("Alguno de los proveedores no existe o está inactivo.");

        if (await _context.Lotes.AnyAsync(l => lotes.Contains(l.NumeroLote)))
            throw new ReglaNegocioException("Alguno de los números de lote ya está registrado.");
    }

    private static IQueryable<ProductoListadoDto> Proyectar(IQueryable<Producto> query) =>
        query.Select(p => new ProductoListadoDto
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Estado = p.Estado,
            Ofertas = p.Lotes
                .Where(l => l.Estado && l.Inventario != null)
                .Select(l => new OfertaDto
                {
                    IdLote = l.Id,
                    NumeroLote = l.NumeroLote,
                    IdProveedor = l.IdProveedor,
                    Proveedor = l.Proveedor.Nombre,
                    Costo = l.Inventario!.Costo,
                    Precio = l.Inventario.Precio,
                    Stock = l.Inventario.Stock
                }).ToList()
        });
}