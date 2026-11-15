using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class ProveedorService : IProveedorService
{
    private readonly InventarioContext _context;

    public ProveedorService(InventarioContext context)
    {
        _context = context;
    }

    public async Task<List<ProveedorDto>> ListarAsync()
    {
        return await _context.Proveedores
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorDto { Id = p.Id, Nombre = p.Nombre })
            .ToListAsync();
    }
}