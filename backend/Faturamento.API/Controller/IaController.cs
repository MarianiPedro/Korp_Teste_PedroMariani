using Faturamento.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IaController : ControllerBase
{
    private readonly IaService _iaService;

    public IaController(IaService iaService)
    {
        _iaService = iaService;
    }

    [HttpGet("sugestoes")]
    public async Task<IActionResult> GetSugestoes()
    {
        var sugestao = await _iaService.SugerirProdutosAsync();
        return Ok(new { sugestao });
    }
}