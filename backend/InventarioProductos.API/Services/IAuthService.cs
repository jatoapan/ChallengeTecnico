using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IAuthService
{
    Task<LoginRespuestaDto?> LoginAsync(LoginDto dto);
}