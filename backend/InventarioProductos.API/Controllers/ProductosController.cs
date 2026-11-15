using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginadoDto<ProductoListadoDto>>> Listar(
        [FromQuery] string? buscar,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        var resultado = await _productoService.ListarAsync(buscar, pagina, tamanoPagina);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoListadoDto>> Obtener(int id)
    {
        return Ok(await _productoService.ObtenerAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ProductoListadoDto>> Crear([FromBody] ProductoGuardarDto dto)
    {
        var creado = await _productoService.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoListadoDto>> Actualizar(int id, [FromBody] ProductoGuardarDto dto)
    {
        return Ok(await _productoService.ActualizarAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productoService.EliminarAsync(id);
        return NoContent();
    }
}