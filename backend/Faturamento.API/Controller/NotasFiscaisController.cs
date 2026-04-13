using Faturamento.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasFiscaisController : ControllerBase
{
    private readonly INotaFiscalService _service;

    public NotasFiscaisController(INotaFiscalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notas = await _service.ListarTodosAsync();
        return Ok(notas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nota = await _service.BuscarPorIdAsync(id);
        return nota is null ? NotFound() : Ok(nota);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarNotaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var criada = await _service.CriarAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
    }

    [HttpPost("{id}/imprimir")]
    public async Task<IActionResult> Imprimir(int id)
    {
        var (sucesso, mensagem) = await _service.ImprimirAsync(id);

        if (!sucesso)
            return Conflict(new { mensagem });

        return Ok(new { mensagem });
    }
}