using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface ILoteService
{
    Task<PaginadoDto<LoteListadoDto>> ListarAsync(FiltroLotesDto filtro, int pagina, int tamanoPagina);
    Task<LoteListadoDto> AgregarAsync(int idProducto, LoteGuardarDto dto);
    Task DesactivarAsync(int idLote);
}
