namespace Faturamento.API.Models;

public class ItemNota
{
    public int Id { get; set; }
    public int NotaFiscalId { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }

    public NotaFiscal NotaFiscal { get; set; } = null!;
}