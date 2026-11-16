using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class InventarioService : IInventarioService
{
    private readonly InventarioContext _context;

    public InventarioService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<OfertaDto> AjustarAsync(int idLote, AjusteInventarioDto dto)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Lote).ThenInclude(l => l.Proveedor)
            .FirstOrDefaultAsync(i => i.IdLote == idLote && i.Lote.Estado)
            ?? throw new NoEncontradoException("El lote no existe.");

        inventario.Precio = dto.Precio;
        inventario.Stock = dto.Stock;
        inventario.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new OfertaDto
        {
            IdLote = idLote,
            NumeroLote = inventario.Lote.NumeroLote,
            IdProveedor = inventario.Lote.IdProveedor,
            Proveedor = inventario.Lote.Proveedor.Nombre,
            Precio = inventario.Precio,
            Stock = inventario.Stock
        };
    }
}