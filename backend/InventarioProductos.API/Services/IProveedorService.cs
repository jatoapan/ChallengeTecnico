using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IProveedorService
{
    Task<List<ProveedorDto>> ListarAsync();
}