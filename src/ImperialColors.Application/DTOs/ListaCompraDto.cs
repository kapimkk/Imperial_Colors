namespace ImperialColors.Application.DTOs;

public class ListaCompraDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public bool Finalizada { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; }
    public List<ItemListaCompraDto> Itens { get; set; } = new();
}

public class ItemListaCompraDto
{
    public int Id { get; set; }
    public int ListaCompraId { get; set; }
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public decimal QuantidadeDesejada { get; set; }
    public decimal? QuantidadeComprada { get; set; }
    public bool Comprado { get; set; }
    public string? Observacoes { get; set; }
    public string Unidade { get; set; } = "UN";
}
