using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IProveedorService
{
    Task<List<ProveedorDto>> ListarAsync(FiltroEstado estado = FiltroEstado.Activos, string? buscar = null);
    Task<ProveedorDto> CrearAsync(ProveedorGuardarDto dto);
    Task<ProveedorDto> ActualizarAsync(int id, ProveedorGuardarDto dto);
    Task DesactivarAsync(int id);
    Task<ProveedorDto> ReactivarAsync(int id);
}
