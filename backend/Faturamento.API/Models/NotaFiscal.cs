namespace Faturamento.API.Models;

public class NotaFiscal
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public StatusNota Status { get; set; } = StatusNota.Aberta;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<ItemNota> Itens { get; set; } = new List<ItemNota>();
}

public enum StatusNota
{
    Aberta = 1,
    Fechada = 2
}