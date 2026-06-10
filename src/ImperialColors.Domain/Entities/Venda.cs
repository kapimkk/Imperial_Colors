using ImperialColors.Domain.Enums;

namespace ImperialColors.Domain.Entities;

public class Venda : BaseEntity
{
    public string NumeroVenda { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.Aberta;
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public string? Observacoes { get; set; }
    public string? Usuario { get; set; }
    public DateTime DataVenda { get; set; } = DateTime.Now;

    public Cliente? Cliente { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();

    public void CalcularTotais()
    {
        Subtotal = Itens.Sum(i => i.Subtotal);
        Total = Subtotal - Desconto;
    }
}
