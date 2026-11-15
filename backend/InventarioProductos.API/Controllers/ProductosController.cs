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
}