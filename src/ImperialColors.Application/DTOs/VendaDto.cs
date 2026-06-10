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
}

public class CriarVendaDto
{
    public int? ClienteId { get; set; }
    public decimal Desconto { get; set; }
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
