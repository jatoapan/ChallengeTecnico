using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IProductoService
{
    Task<PaginadoDto<ProductoListadoDto>> ListarAsync(string? buscar, int pagina, int tamanoPagina, FiltroEstado estado = FiltroEstado.Activos);
    Task<ProductoListadoDto> ObtenerAsync(int id);
    Task<ProductoListadoDto> CrearAsync(ProductoGuardarDto dto);
    Task<ProductoListadoDto> ActualizarAsync(int id, ProductoActualizarDto dto);
    Task EliminarAsync(int id);
    Task<MatrizPreciosDto> ObtenerMatrizPreciosAsync(string? buscar, int pagina, int tamanoPagina);
}