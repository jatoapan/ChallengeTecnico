using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login([FromBody] LoginDto dto)
    {
        var respuesta = await _authService.LoginAsync(dto);

        if (respuesta == null)
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title = "Credenciales inválidas",
                Detail = "El usuario o la contraseña son incorrectos."
            });

        return Ok(respuesta);
    }
}