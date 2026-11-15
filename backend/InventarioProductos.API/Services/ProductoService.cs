using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class ProductoService : IProductoService
{
    private readonly InventarioContext _context;

    public ProductoService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<PaginadoDto<ProductoListadoDto>> ListarAsync(string? buscar, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var query = _context.Productos.AsNoTracking().Where(p => p.Estado);

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(p => p.Nombre.Contains(buscar) || p.Codigo.Contains(buscar));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new ProductoListadoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Ofertas = p.Lotes
                    .Where(l => l.Estado && l.Inventario != null)
                    .Select(l => new OfertaDto
                    {
                        IdLote = l.Id,
                        NumeroLote = l.NumeroLote,
                        IdProveedor = l.IdProveedor,
                        Proveedor = l.Proveedor.Nombre,
                        Precio = l.Inventario!.Precio,
                        Stock = l.Inventario.Stock
                    }).ToList()
            })
            .ToListAsync();

        return new PaginadoDto<ProductoListadoDto>
        {
            Items = items,
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }
}