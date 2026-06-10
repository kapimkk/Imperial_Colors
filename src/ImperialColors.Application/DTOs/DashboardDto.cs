namespace ImperialColors.Application.DTOs;

public class DashboardDto
{
    public decimal TotalVendasHoje { get; set; }
    public decimal TotalVendasMes { get; set; }
    public int QuantidadeVendasHoje { get; set; }
    public int ProdutosEstoqueBaixo { get; set; }
    public int ProdutosSemEstoque { get; set; }
    public int TotalClientes { get; set; }
    public int TotalProdutos { get; set; }
    public List<ProdutoBaixoEstoqueDto> ProdutosBaixoEstoque { get; set; } = new();
    public List<ProdutoMaisVendidoDto> TopProdutosMes { get; set; } = new();

    public int AlertasEstoqueCritico => ProdutosEstoqueBaixo + ProdutosSemEstoque;
}

public class ProdutoBaixoEstoqueDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public string Unidade { get; set; } = "UN";
}
