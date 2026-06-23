namespace ImperialColors.Application.DTOs;

public class LinhaRelatorioVendaExternaDto
{
    public DateTime DataVenda { get; set; }
    public string CodigoVenda { get; set; } = string.Empty;
    public string ProdutoItem { get; set; } = string.Empty;
    public decimal QuantidadeVendida { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

public class ProdutoRankingDto
{
    public int Posicao { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string NomeProduto { get; set; } = string.Empty;
    public decimal QuantidadeTotal { get; set; }
    public decimal FaturamentoGerado { get; set; }
}

public class ProdutoEncalhadoDto
{
    public string CodigoInterno { get; set; } = string.Empty;
    public string NomeProduto { get; set; } = string.Empty;
    public decimal EstoqueAtual { get; set; }
    public decimal ValorTotalParado { get; set; }
}

public enum TipoAnaliseGiroProduto
{
    MaisVendidos,
    MenosVendidos,
    NuncaVendidos
}
