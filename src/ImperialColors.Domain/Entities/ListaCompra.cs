namespace ImperialColors.Domain.Entities;

public class ListaCompra : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public int? FornecedorId { get; set; }
    public bool Finalizada { get; set; }
    public string? Observacoes { get; set; }

    public Fornecedor? Fornecedor { get; set; }
    public ICollection<ItemListaCompra> Itens { get; set; } = new List<ItemListaCompra>();
}
