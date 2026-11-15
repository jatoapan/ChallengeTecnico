using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IProductoService
{
    Task<PaginadoDto<ProductoListadoDto>> ListarAsync(string? buscar, int pagina, int tamanoPagina);
    Task<ProductoListadoDto> ObtenerAsync(int id);
    Task<ProductoListadoDto> CrearAsync(ProductoGuardarDto dto);
}