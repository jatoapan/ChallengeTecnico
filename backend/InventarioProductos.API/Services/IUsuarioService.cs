using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ListarAsync(string? buscar, FiltroEstado estado = FiltroEstado.Activos);
    Task<List<RolDto>> ListarRolesAsync();
    Task<UsuarioDto> CrearAsync(UsuarioGuardarDto dto);
    Task<UsuarioDto> ActualizarAsync(int id, UsuarioGuardarDto dto);
    Task DesactivarAsync(int id);
    Task<UsuarioDto> ReactivarAsync(int id);
}
