using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Autenticación de usuarios.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Valida las credenciales y devuelve un token JWT.
    /// </summary>
    /// <param name="dto">Usuario y contraseña.</param>
    /// <response code="200">Credenciales válidas; devuelve el token, el usuario y su rol.</response>
    /// <response code="400">Faltan datos obligatorios.</response>
    /// <response code="401">Usuario o contraseña incorrectos.</response>
    /// <response code="429">Cuenta bloqueada temporalmente por intentos fallidos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginRespuestaDto>> Login([FromBody] LoginDto dto)
    {
        var respuesta = await _authService.LoginAsync(dto);

        if (respuesta == null)
            return Unauthorized(ProblemaHttp.Crear(HttpContext, 401,
                "Credenciales inválidas", "El usuario o la contraseña son incorrectos."));

        return Ok(respuesta);
    }
}
