using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Historial de cambios (auditoría). Solo lectura.
/// </summary>
[ApiController]
[Authorize(Policy = Politicas.VerAuditoria)]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    /// <summary>
    /// Lista los registros de auditoría, del más reciente al más antiguo, con filtros. Solo Administrador.
    /// </summary>
    /// <param name="filtro">Búsqueda por usuario o detalle, acción, entidad y rango de fechas (todos opcionales).</param>
    /// <param name="pagina">Número de página, desde 1.</param>
    /// <param name="tamanoPagina">Cantidad de registros por página.</param>
    /// <response code="200">Página de registros de auditoría.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<AuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginadoDto<AuditoriaDto>>> Listar(
        [FromQuery] FiltroAuditoriaDto filtro,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        return Ok(await _auditoriaService.ListarAsync(filtro, pagina, tamanoPagina));
    }

    /// <summary>
    /// Cantidad de registros por acción para los filtros aplicados. Solo Administrador.
    /// </summary>
    /// <param name="filtro">Mismos filtros que el listado.</param>
    /// <response code="200">Total y cantidad por acción.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpGet("estadisticas")]
    [ProducesResponseType(typeof(EstadisticasAuditoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EstadisticasAuditoriaDto>> Estadisticas([FromQuery] FiltroAuditoriaDto filtro)
    {
        return Ok(await _auditoriaService.EstadisticasAsync(filtro));
    }
}
