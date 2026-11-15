using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventarioProductos.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProveedorDto>>> Listar()
    {
        return Ok(await _proveedorService.ListarAsync());
    }
}