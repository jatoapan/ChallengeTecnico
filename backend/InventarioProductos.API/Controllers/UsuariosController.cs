using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Gestión de usuarios. Solo Administrador.
/// </summary>
[ApiController]
[Authorize(Policy = Politicas.GestionarUsuarios)]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Lista los usuarios con búsqueda y filtro por estado.
    /// </summary>
    /// <param name="buscar">Texto a buscar en el usuario, nombres, apellidos o correo (opcional).</param>
    /// <param name="estado">Activos (por defecto), Inactivos o Todos.</param>
    /// <response code="200">Lista de usuarios (sin contraseñas).</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar([FromQuery] string? buscar,
        [FromQuery] FiltroEstado estado = FiltroEstado.Activos)
    {
        return Ok(await _usuarioService.ListarAsync(buscar, estado));
    }

    /// <summary>
    /// Lista los roles activos para asignar a un usuario.
    /// </summary>
    /// <response code="200">Lista de roles.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RolDto>>> Roles()
    {
        return Ok(await _usuarioService.ListarRolesAsync());
    }

    /// <summary>
    /// Crea un usuario. La contraseña se guarda cifrada con BCrypt.
    /// </summary>
    /// <param name="dto">Datos del usuario y su contraseña.</param>
    /// <response code="201">Usuario creado.</response>
    /// <response code="400">Datos inválidos, contraseña débil, usuario o correo repetido.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UsuarioDto>> Crear([FromBody] UsuarioGuardarDto dto)
    {
        var creado = await _usuarioService.CrearAsync(dto);
        return StatusCode(StatusCodes.Status201Created, creado);
    }

    /// <summary>
    /// Actualiza un usuario. Si se envía una contraseña, se restablece.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    /// <param name="dto">Datos del usuario; contraseña opcional.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="400">Datos inválidos, duplicados o se dejaría el sistema sin Administrador.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> Actualizar(int id, [FromBody] UsuarioGuardarDto dto)
    {
        return Ok(await _usuarioService.ActualizarAsync(id, dto));
    }

    /// <summary>
    /// Desactiva un usuario (no puede iniciar sesión). No se permite con el propio usuario ni con el último Administrador.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    /// <response code="204">Usuario desactivado.</response>
    /// <response code="400">Es el propio usuario o el último Administrador activo.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _usuarioService.DesactivarAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Reactiva un usuario desactivado.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    /// <response code="200">Usuario reactivado.</response>
    /// <response code="400">El usuario ya está activo.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpPost("{id:int}/reactivar")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> Reactivar(int id)
    {
        return Ok(await _usuarioService.ReactivarAsync(id));
    }
}
