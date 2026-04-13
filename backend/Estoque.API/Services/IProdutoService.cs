using Estoque.API.Models;

namespace Estoque.API.Services;

public interface IProdutoService
{
    Task<IEnumerable<Produto>> ListarTodosAsync();
    Task<Produto?> BuscarPorIdAsync(int id);
    Task<Produto> CriarAsync(Produto produto);
    Task<Produto?> AtualizarAsync(int id, Produto produto);
    Task<bool> DeletarAsync(int id);
    Task<bool> DebitarSaldoAsync(int id, int quantidade);
}