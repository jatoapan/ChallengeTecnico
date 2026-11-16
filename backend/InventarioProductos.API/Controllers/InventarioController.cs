using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Ajustes de precio y stock por lote.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    /// <summary>
    /// Ajusta el precio y el stock de un lote.
    /// </summary>
    /// <param name="idLote">Id del lote (oferta) a ajustar.</param>
    /// <param name="dto">Nuevo precio y stock.</param>
    /// <response code="200">Lote actualizado; devuelve la oferta.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Operador ni Administrador.</response>
    /// <response code="404">El lote no existe.</response>
    [HttpPut("{idLote:int}")]
    [Authorize(Policy = Politicas.AjustarInventario)]
    [ProducesResponseType(typeof(OfertaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfertaDto>> Ajustar(int idLote, [FromBody] AjusteInventarioDto dto)
    {
        return Ok(await _inventarioService.AjustarAsync(idLote, dto));
    }
}
