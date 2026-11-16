using System.Security.Claims;
using System.Text;
using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InventarioProductos.API.Services;

public class AuthService : IAuthService
{
    private readonly InventarioContext _context;
    private readonly IConfiguration _config;
    private readonly IRegistroIntentosLogin _intentos;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditoriaService _auditoria;
    private readonly ILogger<AuthService> _logger;

    public AuthService(InventarioContext context, IConfiguration config, IRegistroIntentosLogin intentos,
        IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoria, ILogger<AuthService> logger)
    {
        _auditoria = auditoria;
        _context = context;
        _config = config;
        _intentos = intentos;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<LoginRespuestaDto?> LoginAsync(LoginDto dto)
    {
        var nombreUsuario = dto.Usuario.Trim();
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        // 1. Si la cuenta está bloqueada no se consulta la base de datos ni se verifica la contraseña.
        var restante = _intentos.TiempoBloqueoRestante(nombreUsuario);
        if (restante != null)
        {
            _logger.LogWarning("Login rechazado: {Usuario} está bloqueado. IP {Ip}", nombreUsuario, ip);
            await _auditoria.RegistrarAccesoAsync("IngresoFallido", null, nombreUsuario, null, "Rechazado: la cuenta está bloqueada");
            throw new DemasiadosIntentosException(
                $"Cuenta bloqueada por intentos fallidos. Intente nuevamente en {Math.Ceiling(restante.Value.TotalMinutes)} minuto(s).");
        }

        // El rol también debe estar activo: si se desactiva un rol, sus usuarios no pueden entrar.
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Estado && u.Rol.Estado);

        // 2. Credenciales incorrectas: se cuenta el intento. Se cuenta aunque el usuario no exista,
        //    así la respuesta no revela qué usuarios existen.
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            var bloqueado = _intentos.RegistrarFallo(nombreUsuario);
            _logger.LogWarning("Login fallido para {Usuario} desde IP {Ip}", nombreUsuario, ip);
            // El motivo exacto queda solo en la auditoría interna; al usuario siempre se le dice lo mismo.
            await _auditoria.RegistrarAccesoAsync("IngresoFallido", usuario?.Id, nombreUsuario, usuario?.Rol.Nombre,
                usuario == null ? "Usuario inexistente o inactivo" : "Contraseña incorrecta");

            if (bloqueado)
            {
                _logger.LogWarning("Cuenta {Usuario} bloqueada {Minutos} minutos por intentos fallidos. IP {Ip}",
                    nombreUsuario, _intentos.MinutosBloqueo, ip);
                await _auditoria.RegistrarAccesoAsync("Bloqueo", usuario?.Id, nombreUsuario, usuario?.Rol.Nombre,
                    $"Cuenta bloqueada {_intentos.MinutosBloqueo} minutos por intentos fallidos");
                throw new DemasiadosIntentosException(
                    $"Demasiados intentos fallidos. La cuenta quedó bloqueada {_intentos.MinutosBloqueo} minuto(s).");
            }

            return null;
        }

        // 3. Login correcto: se reinicia la cuenta de intentos.
        _intentos.Limpiar(nombreUsuario);
        _logger.LogInformation("Login correcto de {Usuario} desde IP {Ip}", nombreUsuario, ip);
        await _auditoria.RegistrarAccesoAsync("Ingreso", usuario.Id, usuario.NombreUsuario, usuario.Rol.Nombre, $"Inicio de sesión ({usuario.Rol.Nombre})");

        var expira = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:MinutosExpiracion"]!));
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Clave"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
            }),
            Issuer = _config["Jwt:Emisor"],
            Audience = _config["Jwt:Audiencia"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        return new LoginRespuestaDto
        {
            Token = new JsonWebTokenHandler().CreateToken(descriptor),
            Usuario = usuario.NombreUsuario,
            Rol = usuario.Rol.Nombre,
            Expira = expira
        };
    }
}