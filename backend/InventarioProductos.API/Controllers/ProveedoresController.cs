using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Gestión de proveedores.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    /// <summary>
    /// Lista los proveedores con sus datos de contacto y cuántos lotes activos tienen.
    /// </summary>
    /// <param name="estado">Activos (por defecto), Inactivos o Todos.</param>
    /// <param name="buscar">Texto a buscar en el nombre o el correo (opcional).</param>
    /// <response code="200">Lista de proveedores.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ProveedorDto>>> Listar([FromQuery] FiltroEstado estado = FiltroEstado.Activos,
        [FromQuery] string? buscar = null)
    {
        return Ok(await _proveedorService.ListarAsync(estado, buscar));
    }

    /// <summary>
    /// Crea un proveedor. Solo Administrador.
    /// </summary>
    /// <param name="dto">Nombre, correo y celular.</param>
    /// <response code="201">Proveedor creado.</response>
    /// <response code="400">Datos inválidos o nombre repetido.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.GestionarProveedores)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProveedorDto>> Crear([FromBody] ProveedorGuardarDto dto)
    {
        var creado = await _proveedorService.CrearAsync(dto);
        return StatusCode(StatusCodes.Status201Created, creado);
    }

    /// <summary>
    /// Actualiza los datos de un proveedor. Solo Administrador.
    /// </summary>
    /// <param name="id">Id del proveedor.</param>
    /// <param name="dto">Nombre, correo y celular.</param>
    /// <response code="200">Proveedor actualizado.</response>
    /// <response code="400">Datos inválidos o nombre repetido.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El proveedor no existe.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.GestionarProveedores)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorDto>> Actualizar(int id, [FromBody] ProveedorGuardarDto dto)
    {
        return Ok(await _proveedorService.ActualizarAsync(id, dto));
    }

    /// <summary>
    /// Desactiva un proveedor (borrado lógico). También desactiva sus lotes; al reactivarlo vuelven sus lotes y productos. Solo Administrador.
    /// </summary>
    /// <param name="id">Id del proveedor.</param>
    /// <response code="204">Proveedor desactivado.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El proveedor no existe.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.GestionarProveedores)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _proveedorService.DesactivarAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Reactiva un proveedor desactivado. Solo Administrador.
    /// </summary>
    /// <param name="id">Id del proveedor.</param>
    /// <response code="200">Proveedor reactivado.</response>
    /// <response code="400">El proveedor ya está activo.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El proveedor no existe.</response>
    [HttpPost("{id:int}/reactivar")]
    [Authorize(Policy = Politicas.GestionarProveedores)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorDto>> Reactivar(int id)
    {
        return Ok(await _proveedorService.ReactivarAsync(id));
    }
}
