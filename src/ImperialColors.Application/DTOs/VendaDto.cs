using ImperialColors.Application.Helpers;
using ImperialColors.Domain.Enums;

namespace ImperialColors.Application.DTOs;

public class VendaDto
{
    public int Id { get; set; }
    public string NumeroVenda { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public StatusVenda Status { get; set; }
    public string StatusDescricao => Status switch
    {
        StatusVenda.Aberta => "Aberta",
        StatusVenda.Finalizada => "Finalizada",
        StatusVenda.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public int QuantidadeParcelas { get; set; } = 1;
    public decimal ValorPago { get; set; }
    public decimal Troco { get; set; }
    public string FormaPagamentoDescricao => PagamentoHelper.ObterDescricao(FormaPagamento, QuantidadeParcelas);
    public string? Observacoes { get; set; }
    public string? Usuario { get; set; }
    public DateTime DataVenda { get; set; }
    public List<ItemVendaDto> Itens { get; set; } = new();
}

public class ItemVendaDto
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? CodigoInterno { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Desconto { get; set; }
    public decimal Subtotal { get; set; }
    public string Unidade { get; set; } = "UN";

    /// <summary>Nome comercial legível para seleção em telas (ex.: Registrar Troca).</summary>
    public string DescricaoTroca => string.IsNullOrWhiteSpace(NomeProduto)
        ? CodigoInterno ?? "Produto da venda"
        : NomeProduto;

    public string NomeExibicao => $"{NomeProduto}  ×{Quantidade} {Unidade}  — R$ {PrecoUnitario:N2}/un";
}

public class CriarVendaDto
{
    public int? ClienteId { get; set; }
    public decimal Desconto { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;
    public int QuantidadeParcelas { get; set; } = 1;
    public decimal ValorPago { get; set; }
    public decimal Troco { get; set; }
    public string? Observacoes { get; set; }
    public string? Usuario { get; set; }
    public List<CriarItemVendaDto> Itens { get; set; } = new();
}

public class CriarItemVendaDto
{
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Desconto { get; set; }
}
