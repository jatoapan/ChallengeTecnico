using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IResumenService
{
    Task<ResumenDto> ObtenerAsync(int umbral, int maximoPorReponer);
}
