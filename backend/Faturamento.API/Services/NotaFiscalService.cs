
using Faturamento.API.Data;
using Faturamento.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.API.Services;

public class NotaFiscalService : INotaFiscalService
{
    private readonly FaturamentoDbContext _context;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotaFiscalService> _logger;

    public NotaFiscalService(FaturamentoDbContext context,IHttpClientFactory httpClientFactory,ILogger<NotaFiscalService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<NotaFiscal>> ListarTodosAsync()
    {
        return await _context.NotasFiscais
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();
    }

    public async Task<NotaFiscal?> BuscarPorIdAsync(int id)
    {
         return await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<NotaFiscal> CriarAsync(CriarNotaRequest request)
    {
        var proximoNumero = await _context.NotasFiscais
            .AnyAsync()
            ? await _context.NotasFiscais.MaxAsync(n => n.Numero) + 1
            : 1;

        var nota = new NotaFiscal
        {
            Numero = proximoNumero,
            Status = StatusNota.Aberta,
            DataCriacao = DateTime.UtcNow,
            Itens = request.Itens.Select(i => new ItemNota
            {
                ProdutoId = i.ProdutoId,
                ProdutoDescricao = i.ProdutoDescricao,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _context.NotasFiscais.Add(nota);
        await _context.SaveChangesAsync();
        return nota;
    }

    public async Task<(bool sucesso, string mensagem)> ImprimirAsync(int id)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota is null)
            return (false, "Nota fiscal não encontrada.");

        if (nota.Status != StatusNota.Aberta)
            return (false, "Apenas notas com status Aberta podem ser impressas.");

        // Debitar saldo de cada produto no Estoque.API
        var client = _httpClientFactory.CreateClient("EstoqueApi");

        foreach (var item in nota.Itens)
        {
            try
            {
                var response = await client.PostAsJsonAsync(
                    $"api/produtos/{item.ProdutoId}/debitar",
                    new { Quantidade = item.Quantidade });

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Falha ao debitar produto {Id}: {Erro}", item.ProdutoId, erro);
                    return (false, $"Saldo insuficiente para o produto '{item.ProdutoDescricao}'.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Estoque.API indisponível: {Msg}", ex.Message);
                return (false, "Serviço de estoque indisponível. Tente novamente em instantes.");
            }
        }

        // Todos os débitos OK — fechar a nota
        nota.Status = StatusNota.Fechada;
        await _context.SaveChangesAsync();

        return (true, "Nota impressa e fechada com sucesso.");
    }
}