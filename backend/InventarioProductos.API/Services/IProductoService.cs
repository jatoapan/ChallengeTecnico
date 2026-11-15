using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IProductoService
{
    Task<PaginadoDto<ProductoListadoDto>> ListarAsync(string? buscar, int pagina, int tamanoPagina);
}