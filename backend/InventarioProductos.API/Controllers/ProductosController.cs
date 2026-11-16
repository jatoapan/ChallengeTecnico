using InventarioProductos.API.Common;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventarioProductos.API.Controllers;

/// <summary>
/// CRUD de productos con sus ofertas por proveedor y lote.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    /// <summary>
    /// Lista los productos con paginación, búsqueda y filtro por estado.
    /// </summary>
    /// <param name="buscar">Texto a buscar en el código o el nombre (opcional).</param>
    /// <param name="pagina">Número de página, desde 1.</param>
    /// <param name="tamanoPagina">Cantidad de productos por página.</param>
    /// <param name="estado">Activos (por defecto), Inactivos o Todos.</param>
    /// <response code="200">Página de productos con sus ofertas.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<ProductoListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginadoDto<ProductoListadoDto>>> Listar(
        [FromQuery] string? buscar,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] FiltroEstado estado = FiltroEstado.Activos)
    {
        var resultado = await _productoService.ListarAsync(buscar, pagina, tamanoPagina, estado);
        return Ok(resultado);
    }

    /// <summary>
    /// Matriz de precios producto × proveedor, para comparar proveedores.
    /// </summary>
    /// <remarks>
    /// Las columnas son los proveedores activos. Por cada proveedor se devuelve el precio menor entre sus
    /// lotes con stock (o el menor de todos si ninguno tiene stock) y el stock total. Si no tiene oferta,
    /// no aparece en "precios".
    /// </remarks>
    /// <param name="buscar">Texto a buscar en el código o el nombre (opcional).</param>
    /// <param name="pagina">Número de página, desde 1.</param>
    /// <param name="tamanoPagina">Cantidad de productos por página.</param>
    /// <response code="200">Proveedores (columnas) y una fila de precios por producto.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    [HttpGet("matriz-precios")]
    [ProducesResponseType(typeof(MatrizPreciosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MatrizPreciosDto>> MatrizPrecios(
        [FromQuery] string? buscar,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        return Ok(await _productoService.ObtenerMatrizPreciosAsync(buscar, pagina, tamanoPagina));
    }

    /// <summary>
    /// Obtiene un producto por su id.
    /// </summary>
    /// <param name="id">Id del producto.</param>
    /// <response code="200">Producto con sus ofertas.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="404">El producto no existe.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoListadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoListadoDto>> Obtener(int id)
    {
        return Ok(await _productoService.ObtenerAsync(id));
    }

    /// <summary>
    /// Crea un producto con su lote o lotes iniciales. Solo Administrador.
    /// </summary>
    /// <param name="dto">Datos del producto y sus ofertas.</param>
    /// <response code="201">Producto creado.</response>
    /// <response code="400">Datos inválidos, código o lote repetido, o proveedor inexistente.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.GestionarProductos)]
    [ProducesResponseType(typeof(ProductoListadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductoListadoDto>> Crear([FromBody] ProductoGuardarDto dto)
    {
        var creado = await _productoService.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// Actualiza los datos del producto (código, nombre y descripción). Solo Administrador.
    /// </summary>
    /// <remarks>
    /// Los lotes no se modifican aquí: se agregan con POST /api/Productos/{id}/lotes, se desactivan con
    /// DELETE /api/Lotes/{id} y su precio y stock se ajustan con PUT /api/Inventario/{idLote}.
    /// </remarks>
    /// <param name="id">Id del producto.</param>
    /// <param name="dto">Datos del producto.</param>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="400">Datos inválidos o reglas de negocio no cumplidas.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El producto no existe.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.GestionarProductos)]
    [ProducesResponseType(typeof(ProductoListadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoListadoDto>> Actualizar(int id, [FromBody] ProductoActualizarDto dto)
    {
        return Ok(await _productoService.ActualizarAsync(id, dto));
    }

    /// <summary>
    /// Desactiva un producto y todos sus lotes (borrado lógico). Solo Administrador.
    /// </summary>
    /// <param name="id">Id del producto.</param>
    /// <response code="204">Producto eliminado.</response>
    /// <response code="401">No hay sesión o el token expiró.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">El producto no existe.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.GestionarProductos)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productoService.EliminarAsync(id);
        return NoContent();
    }
}
