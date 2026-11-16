using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class InventarioService : IInventarioService
{
    private readonly InventarioContext _context;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IAuditoriaService _auditoria;
    private readonly ILogger<InventarioService> _logger;

    public InventarioService(InventarioContext context, IUsuarioActual usuarioActual,
        IAuditoriaService auditoria, ILogger<InventarioService> logger)
    {
        _context = context;
        _usuarioActual = usuarioActual;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<OfertaDto> AjustarAsync(int idLote, AjusteInventarioDto dto)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Lote).ThenInclude(l => l.Proveedor)
            .FirstOrDefaultAsync(i => i.IdLote == idLote && i.Lote.Estado)
            ?? throw new NoEncontradoException("El lote no existe.");

        var costoAnterior = inventario.Costo;
        var precioAnterior = inventario.Precio;
        var stockAnterior = inventario.Stock;

        inventario.Costo = dto.Costo;
        inventario.Precio = dto.Precio;
        inventario.Stock = dto.Stock;
        inventario.FechaActualizacion = DateTime.Now;

        _auditoria.Registrar("Ajustar", "Lote", idLote,
            $"Lote {inventario.Lote.NumeroLote}: costo {costoAnterior:0.00} -> {inventario.Costo:0.00}, precio {precioAnterior:0.00} -> {inventario.Precio:0.00}, stock {stockAnterior} -> {inventario.Stock}");

        // Un solo SaveChanges: el ajuste y su auditoría se guardan juntos.
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Auditoría: {Usuario} ajustó el lote {IdLote} ({NumeroLote}): precio {PrecioAnterior} → {PrecioNuevo}, stock {StockAnterior} → {StockNuevo}",
            _usuarioActual.Nombre, idLote, inventario.Lote.NumeroLote, precioAnterior, inventario.Precio, stockAnterior, inventario.Stock);

        return new OfertaDto
        {
            IdLote = idLote,
            NumeroLote = inventario.Lote.NumeroLote,
            IdProveedor = inventario.Lote.IdProveedor,
            Proveedor = inventario.Lote.Proveedor.Nombre,
            Costo = inventario.Costo,
            Precio = inventario.Precio,
            Stock = inventario.Stock
        };
    }
}