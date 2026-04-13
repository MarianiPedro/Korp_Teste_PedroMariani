using Faturamento.API.Models;

namespace Faturamento.API.Services;

public interface INotaFiscalService
{
    Task<IEnumerable<NotaFiscal>> ListarTodosAsync();
    Task<NotaFiscal?> BuscarPorIdAsync(int id);
    Task<NotaFiscal> CriarAsync(CriarNotaRequest request);
    Task<(bool sucesso, string mensagem)> ImprimirAsync(int id);
}
public record CriarNotaRequest(
    List<ItemNotaRequest> Itens
);

public record ItemNotaRequest(
    int ProdutoId,
    string ProdutoDescricao,
    int Quantidade
);