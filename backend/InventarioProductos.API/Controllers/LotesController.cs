using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// Lotes de inventario: cada lote es un producto ofrecido por un proveedor, con su precio y stock.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class LotesController : ControllerBase
{
    private readonly ILoteService _loteService;

    public LotesController(ILoteService loteService)
    {
        _loteService = loteService;
    }

    /// <summary>
    /// Lista los lotes activos (una fila por producto, proveedor y lote) con paginación y búsqueda.
    /// </summary>
    /// <param name="filtro">Filtros opcionales: buscar (texto), idProveedor, stockMin, stockMax, precioMin y precioMax.</param>
    /// <param name="pagina">Número de página, desde 1.</param>
    /// <param name="tamanoPagina">Cantidad de lotes por página.</param>
    /// <response code="200">Página de lotes.</response>
    /// <response code="400">Un filtro es inválido (negativo o con el mínimo mayor que el máximo).</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<LoteListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginadoDto<LoteListadoDto>>> Listar(
        [FromQuery] FiltroLotesDto filtro,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        return Ok(await _loteService.ListarAsync(filtro, pagina, tamanoPagina));
    }

    /// <summary>
    /// Agrega un lote (proveedor, número de lote, precio y stock) a un producto. Solo Administrador.
    /// </summary>
    /// <param name="idProducto">Id del producto.</param>
    /// <param name="dto">Datos del lote.</param>
    /// <response code="201">Lote creado.</response>
    /// <response code="400">Datos inválidos, proveedor inexistente o número de lote repetido.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El producto no existe.</response>
    [HttpPost("/api/Productos/{idProducto:int}/lotes")]
    [Authorize(Policy = Politicas.GestionarProductos)]
    [ProducesResponseType(typeof(LoteListadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoteListadoDto>> Agregar(int idProducto, [FromBody] LoteGuardarDto dto)
    {
        var lote = await _loteService.AgregarAsync(idProducto, dto);
        return StatusCode(StatusCodes.Status201Created, lote);
    }

    /// <summary>
    /// Desactiva un lote (borrado lógico). No se permite si es el único lote activo del producto. Solo Administrador.
    /// </summary>
    /// <param name="id">Id del lote.</param>
    /// <response code="204">Lote desactivado.</response>
    /// <response code="400">Es el único lote activo del producto.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El lote no existe.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.GestionarProductos)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _loteService.DesactivarAsync(id);
        return NoContent();
    }
}
