using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Resumen del inventario para la página de inicio.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class ResumenController : ControllerBase
{
    private readonly IResumenService _resumenService;

    public ResumenController(IResumenService resumenService)
    {
        _resumenService = resumenService;
    }

    /// <summary>
    /// Totales del inventario y lotes con stock bajo.
    /// </summary>
    /// <param name="umbral">Stock máximo para considerar un lote con stock bajo (por defecto 10).</param>
    /// <param name="maximo">Cantidad máxima de lotes por reponer a devolver (por defecto 8).</param>
    /// <response code="200">Resumen del inventario.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResumenDto>> Obtener([FromQuery] int umbral = 10, [FromQuery] int maximo = 8)
    {
        return Ok(await _resumenService.ObtenerAsync(umbral, maximo));
    }
}
