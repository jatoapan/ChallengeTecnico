using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class ResumenService : IResumenService
{
    private readonly InventarioContext _context;

    public ResumenService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<ResumenDto> ObtenerAsync(int umbral, int maximoPorReponer)
    {
        umbral = Math.Clamp(umbral, 0, 1000);
        maximoPorReponer = Math.Clamp(maximoPorReponer, 1, 50);

        // Lotes activos de productos activos que tienen inventario. Las consultas son de solo lectura
        // (AsNoTracking) y cada total se calcula en SQL, sin traer las filas a memoria.
        var lotes = _context.Lotes
            .AsNoTracking()
            .Where(l => l.Estado && l.Producto.Estado && l.Inventario != null);

        var resumen = new ResumenDto
        {
            Umbral = umbral,
            TotalProductos = await _context.Productos.CountAsync(p => p.Estado),
            TotalProveedores = await _context.Proveedores.CountAsync(p => p.Estado),
            StockTotal = await lotes.SumAsync(l => (int?)l.Inventario!.Stock) ?? 0,
            LotesStockBajo = await lotes.CountAsync(l => l.Inventario!.Stock <= umbral)
        };

        resumen.PorReponer = await Proyectar(lotes
            .Where(l => l.Inventario!.Stock <= umbral)
            .OrderBy(l => l.Inventario!.Stock).ThenBy(l => l.Producto.Nombre)
            .Take(maximoPorReponer))
            .ToListAsync();

        resumen.UltimosLotes = await Proyectar(lotes
            .OrderByDescending(l => l.Inventario!.FechaActualizacion).ThenByDescending(l => l.Id)
            .Take(5))
            .ToListAsync();

        return resumen;
    }

    private static IQueryable<LotePorReponerDto> Proyectar(IQueryable<Lote> lotes) =>
        lotes.Select(l => new LotePorReponerDto
        {
            IdLote = l.Id,
            NumeroLote = l.NumeroLote,
            IdProducto = l.IdProducto,
            CodigoProducto = l.Producto.Codigo,
            Producto = l.Producto.Nombre,
            Proveedor = l.Proveedor.Nombre,
            Precio = l.Inventario!.Precio,
            Stock = l.Inventario.Stock,
            FechaActualizacion = l.Inventario.FechaActualizacion
        });
}
