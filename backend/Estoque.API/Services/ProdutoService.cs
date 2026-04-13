
using Estoque.API.Data;
using Estoque.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Estoque.API.Services;

public class ProdutoService : IProdutoService
{
    private readonly EstoqueDbContext _context;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(EstoqueDbContext context, ILogger<ProdutoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Produto>> ListarTodosAsync()
    {
        return await _context.Produtos
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    public async Task<Produto?> BuscarPorIdAsync(int id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<Produto> CriarAsync(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<Produto?> AtualizarAsync(int id, Produto produto)
    {
        var existente = await _context.Produtos.FindAsync(id);
        if (existente is null) return null;

        existente.Codigo = produto.Codigo;
        existente.Descricao = produto.Descricao;
        existente.Saldo = produto.Saldo;

        await _context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return false;

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DebitarSaldoAsync(int id, int quantidade)
    {
        // Busca com tracking para o lock otimista funcionar
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null || produto.Saldo < quantidade) return false;

        produto.Saldo -= quantidade;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Outro processo alterou o registro ao mesmo tempo
            _logger.LogWarning("Conflito de concorrência ao debitar produto {Id}: {Msg}", id, ex.Message);
            return false;
        }
    }
}