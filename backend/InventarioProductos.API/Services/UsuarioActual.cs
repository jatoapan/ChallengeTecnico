using System.Security.Claims;

namespace InventarioProductos.API.Services;

// Lee el usuario del JWT. Los servicios dependen de IUsuarioActual y no de HttpContext,
// así se pueden probar con un usuario falso.
public class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Usuario => _httpContextAccessor.HttpContext?.User;

    // Solo se usa en endpoints autenticados; si falta el claim es un error de configuración.
    public int Id => int.TryParse(Usuario?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new InvalidOperationException("No hay un usuario autenticado en la petición.");

    public string Nombre => Usuario?.Identity?.Name ?? "desconocido";

    public string? Rol => Usuario?.FindFirstValue(ClaimTypes.Role);
}
