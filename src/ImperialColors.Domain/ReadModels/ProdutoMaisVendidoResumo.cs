namespace ImperialColors.Domain.ReadModels;

public class ProdutoMaisVendidoResumo
{
    public string NomeProduto { get; set; } = string.Empty;
    public decimal QuantidadeTotal { get; set; }
    public decimal TotalVendido { get; set; }
}
