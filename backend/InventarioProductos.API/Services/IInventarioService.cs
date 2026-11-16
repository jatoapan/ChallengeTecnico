using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IInventarioService
{
    Task<OfertaDto> AjustarAsync(int idLote, AjusteInventarioDto dto);
}