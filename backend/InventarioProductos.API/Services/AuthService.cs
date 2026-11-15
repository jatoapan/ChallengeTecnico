using System.Security.Claims;
using System.Text;
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

    public AuthService(InventarioContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<LoginRespuestaDto?> LoginAsync(LoginDto dto)
    {
        var nombreUsuario = dto.Usuario.Trim();

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Estado);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return null;

        var expira = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:MinutosExpiracion"]!));
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Clave"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol)
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
            Rol = usuario.Rol,
            Expira = expira
        };
    }
}