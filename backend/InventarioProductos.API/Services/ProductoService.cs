using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
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
                Inventario = new Inventario { Precio = o.Precio, Stock = o.Stock }
            }).ToList()
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return await ObtenerAsync(producto.Id);
    }

    public async Task<ProductoListadoDto> ActualizarAsync(int id, ProductoGuardarDto dto)
    {
        var producto = await _context.Productos
            .Include(p => p.Lotes).ThenInclude(l => l.Inventario)
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El producto no existe.");

        var codigo = dto.Codigo.Trim().ToUpper();

        if (await _context.Productos.AnyAsync(p => p.Codigo == codigo && p.Id != id))
            throw new ReglaNegocioException("Ya existe un producto con ese código.");

        var idsConservados = dto.Ofertas.Where(o => o.IdLote != null).Select(o => o.IdLote!.Value).ToList();

        if (idsConservados.Any(i => !producto.Lotes.Any(l => l.Id == i)))
            throw new ReglaNegocioException("Alguno de los lotes no pertenece al producto.");

        await ValidarOfertasAsync(dto.Ofertas, idsConservados);

        producto.Codigo = codigo;
        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion?.Trim();

        foreach (var lote in producto.Lotes.Where(l => l.Estado && !idsConservados.Contains(l.Id)))
            lote.Estado = false;

        foreach (var oferta in dto.Ofertas)
        {
            if (oferta.IdLote == null)
            {
                producto.Lotes.Add(new Lote
                {
                    IdProveedor = oferta.IdProveedor,
                    NumeroLote = oferta.NumeroLote.Trim().ToUpper(),
                    Estado = true,
                    Inventario = new Inventario { Precio = oferta.Precio, Stock = oferta.Stock }
                });
                continue;
            }

            var existente = producto.Lotes.First(l => l.Id == oferta.IdLote);
            existente.IdProveedor = oferta.IdProveedor;
            existente.NumeroLote = oferta.NumeroLote.Trim().ToUpper();
            existente.Estado = true;
            existente.Inventario!.Precio = oferta.Precio;
            existente.Inventario.Stock = oferta.Stock;
            existente.Inventario.FechaActualizacion = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return await ObtenerAsync(id);
    }

    public async Task EliminarAsync(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id && p.Estado)
            ?? throw new NoEncontradoException("El producto no existe.");

        producto.Estado = false;
        await _context.SaveChangesAsync();
    }

    private async Task ValidarOfertasAsync(List<OfertaGuardarDto> ofertas, List<int>? idsLotesPropios = null)
    {
        var lotes = ofertas.Select(o => o.NumeroLote.Trim().ToUpper()).ToList();

        if (lotes.Distinct().Count() != lotes.Count)
            throw new ReglaNegocioException("Hay números de lote repetidos en la solicitud.");

        var idsProveedor = ofertas.Select(o => o.IdProveedor).Distinct().ToList();
        var existentes = await _context.Proveedores.CountAsync(p => idsProveedor.Contains(p.Id) && p.Estado);

        if (existentes != idsProveedor.Count)
            throw new ReglaNegocioException("Alguno de los proveedores no existe o está inactivo.");

        var propios = idsLotesPropios ?? new List<int>();

        if (await _context.Lotes.AnyAsync(l => lotes.Contains(l.NumeroLote) && !propios.Contains(l.Id)))
            throw new ReglaNegocioException("Alguno de los números de lote ya está registrado.");
    }

    private static IQueryable<ProductoListadoDto> Proyectar(IQueryable<Producto> query) =>
        query.Select(p => new ProductoListadoDto
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
        });
}