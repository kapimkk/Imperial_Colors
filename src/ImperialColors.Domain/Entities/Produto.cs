namespace ImperialColors.Domain.Entities;

public class Produto : BaseEntity
{
    public string CodigoInterno { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public int? MarcaId { get; set; }
    public decimal QuantidadeEstoque { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public string Unidade { get; set; } = "UN";
    public decimal Custo { get; set; }
    public decimal PrecoVenda { get; set; }
    public string? Observacoes { get; set; }

    public Categoria? Categoria { get; set; }
    public Marca? Marca { get; set; }
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
    public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
    public ICollection<ItemListaCompra> ItensListaCompra { get; set; } = new List<ItemListaCompra>();

    public bool EstoqueBaixo => QuantidadeEstoque <= EstoqueMinimo;
    public bool SemEstoque => QuantidadeEstoque <= 0;
}
