using InventarioProductos.API.DTOs;
using InventarioProductos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    [HttpPut("{idLote:int}")]
    public async Task<ActionResult<OfertaDto>> Ajustar(int idLote, [FromBody] AjusteInventarioDto dto)
    {
        return Ok(await _inventarioService.AjustarAsync(idLote, dto));
    }
}