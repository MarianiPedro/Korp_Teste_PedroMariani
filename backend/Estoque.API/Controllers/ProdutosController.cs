using Estoque.API.Models;
using Estoque.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var produtos = await _service.ListarTodosAsync();
        return Ok(produtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var produto = await _service.BuscarPorIdAsync(id);
        return produto is null ? NotFound() : Ok(produto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Produto produto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var criado = await _service.CriarAsync(produto);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Produto produto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var atualizado = await _service.AtualizarAsync(id, produto);
        return atualizado is null ? NotFound() : Ok(atualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sucesso = await _service.DeletarAsync(id);
        return sucesso ? NoContent() : NotFound();
    }

    // Endpoint interno chamado pelo Faturamento.API
    [HttpPost("{id}/debitar")]
    public async Task<IActionResult> Debitar(int id, [FromBody] DebitarRequest request)
    {
        var sucesso = await _service.DebitarSaldoAsync(id, request.Quantidade);

        if (!sucesso)
            return Conflict(new { mensagem = "Saldo insuficiente ou conflito de concorrência." });

        return Ok(new { mensagem = "Saldo debitado com sucesso." });
    }
}

public record DebitarRequest(int Quantidade);