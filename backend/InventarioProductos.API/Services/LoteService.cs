using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

// Operaciones sobre lotes: cada una es pequeña, se guarda en un solo SaveChanges junto con su
// registro de auditoría y queda en Serilog.
public class LoteService : ILoteService
{
    private readonly InventarioContext _context;
    private readonly IAuditoriaService _auditoria;
    private readonly IUsuarioActual _usuarioActual;
    private readonly ILogger<LoteService> _logger;

    public LoteService(InventarioContext context, IAuditoriaService auditoria,
        IUsuarioActual usuarioActual, ILogger<LoteService> logger)
    {
        _context = context;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _logger = logger;
    }

    public async Task<PaginadoDto<LoteListadoDto>> ListarAsync(FiltroLotesDto filtro, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        if (filtro.StockMin > filtro.StockMax)
            throw new ReglaNegocioException("El stock mínimo no puede ser mayor que el máximo.");
        if (filtro.PrecioMin > filtro.PrecioMax)
            throw new ReglaNegocioException("El precio mínimo no puede ser mayor que el máximo.");

        // Solo lotes activos de productos activos (las bajas se hacen en cascada).
        var query = _context.Lotes
            .AsNoTracking()
            .Where(l => l.Estado && l.Producto.Estado && l.Inventario != null);

        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var texto = filtro.Buscar.Trim();
            query = query.Where(l =>
                l.Producto.Codigo.Contains(texto) ||
                l.Producto.Nombre.Contains(texto) ||
                l.Proveedor.Nombre.Contains(texto) ||
                l.NumeroLote.Contains(texto));
        }

        // Filtros opcionales: cada uno se aplica solo si viene con valor.
        if (filtro.IdProveedor is > 0)
            query = query.Where(l => l.IdProveedor == filtro.IdProveedor);
        if (filtro.StockMin is not null)
            query = query.Where(l => l.Inventario!.Stock >= filtro.StockMin);
        if (filtro.StockMax is not null)
            query = query.Where(l => l.Inventario!.Stock <= filtro.StockMax);
        if (filtro.PrecioMin is not null)
            query = query.Where(l => l.Inventario!.Precio >= filtro.PrecioMin);
        if (filtro.PrecioMax is not null)
            query = query.Where(l => l.Inventario!.Precio <= filtro.PrecioMax);

        var total = await query.CountAsync();

        var items = await Proyectar(query
                .OrderBy(l => l.Producto.Nombre).ThenBy(l => l.Proveedor.Nombre).ThenBy(l => l.NumeroLote)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina))
            .ToListAsync();

        return new PaginadoDto<LoteListadoDto>
        {
            Items = items,
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    public async Task<LoteListadoDto> AgregarAsync(int idProducto, LoteGuardarDto dto)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == idProducto && p.Estado)
            ?? throw new NoEncontradoException("El producto no existe.");

        var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.IdProveedor && p.Estado)
            ?? throw new ReglaNegocioException("El proveedor no existe o está inactivo.");

        var numeroLote = dto.NumeroLote.Trim().ToUpper();

        if (await _context.Lotes.AnyAsync(l => l.NumeroLote == numeroLote))
            throw new ReglaNegocioException("El número de lote ya está registrado.");

        var lote = new Lote
        {
            IdProducto = idProducto,
            IdProveedor = dto.IdProveedor,
            NumeroLote = numeroLote,
            Estado = true,
            Inventario = new Inventario { Costo = dto.Costo, Precio = dto.Precio, Stock = dto.Stock }
        };

        // El id del lote se conoce después del primer guardado: transacción para que el lote
        // y su auditoría queden juntos o no quede ninguno.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        _context.Lotes.Add(lote);
        await _context.SaveChangesAsync();

        _auditoria.Registrar("Crear", "Lote", lote.Id,
            $"Lote {numeroLote} de {producto.Codigo} con {proveedor.Nombre}: costo {dto.Costo:0.00}, precio {dto.Precio:0.00}, stock {dto.Stock}");
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();

        _logger.LogInformation(
            "Auditoría: {Usuario} agregó el lote {IdLote} ({NumeroLote}) al producto {Codigo} con {Proveedor}",
            _usuarioActual.Nombre, lote.Id, numeroLote, producto.Codigo, proveedor.Nombre);

        return await Proyectar(_context.Lotes.AsNoTracking().Where(l => l.Id == lote.Id)).FirstAsync();
    }

    public async Task DesactivarAsync(int idLote)
    {
        var lote = await _context.Lotes
            .Include(l => l.Producto)
            .FirstOrDefaultAsync(l => l.Id == idLote && l.Estado && l.Producto.Estado)
            ?? throw new NoEncontradoException("El lote no existe.");

        // Un producto activo siempre debe tener al menos una oferta.
        var lotesActivos = await _context.Lotes.CountAsync(l => l.IdProducto == lote.IdProducto && l.Estado);
        if (lotesActivos <= 1)
            throw new ReglaNegocioException(
                "Es el único lote del producto. Para quitarlo, desactive el producto completo.");

        lote.Estado = false;
        _auditoria.Registrar("Eliminar", "Lote", idLote, $"Lote {lote.NumeroLote} de {lote.Producto.Codigo} desactivado");
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Auditoría: {Usuario} desactivó el lote {IdLote} ({NumeroLote}) del producto {Codigo}",
            _usuarioActual.Nombre, idLote, lote.NumeroLote, lote.Producto.Codigo);
    }

    private static IQueryable<LoteListadoDto> Proyectar(IQueryable<Lote> query) =>
        query.Select(l => new LoteListadoDto
        {
            IdLote = l.Id,
            NumeroLote = l.NumeroLote,
            IdProducto = l.IdProducto,
            CodigoProducto = l.Producto.Codigo,
            Producto = l.Producto.Nombre,
            IdProveedor = l.IdProveedor,
            Proveedor = l.Proveedor.Nombre,
            Costo = l.Inventario!.Costo,
            Precio = l.Inventario.Precio,
            Stock = l.Inventario.Stock,
            FechaActualizacion = l.Inventario.FechaActualizacion
        });
}
