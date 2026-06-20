namespace ImperialColors.Application.DTOs;

public class VendaExternaDto
{
    public int Id { get; set; }
    public string NumeroVendaExterna { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string? Observacoes { get; set; }
    public string? Usuario { get; set; }
    public DateTime DataVenda { get; set; }
    public int TotalItens => Itens.Count;
    public List<ItemVendaExternaDto> Itens { get; set; } = new();
}

public class ItemVendaExternaDto
{
    public int Id { get; set; }
    public int VendaExternaId { get; set; }
    public int? ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoBase { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public bool ItemManual => !ProdutoId.HasValue;
    public string TipoDescricao => ItemManual ? "Manual" : "Estoque";
}

public class RegistrarVendaExternaDto
{
    public string? Observacoes { get; set; }
    public string? Usuario { get; set; }
    public List<RegistrarItemVendaExternaDto> Itens { get; set; } = new();
}

public class RegistrarItemVendaExternaDto
{
    public int? ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoBase { get; set; }
    public decimal PrecoUnitario { get; set; }
}

public class LinhaImportacaoVendaExternaDto
{
    public int NumeroLinha { get; set; }
    public string? CodigoBarras { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public int? ProdutoId { get; set; }
    public decimal PrecoBase { get; set; }
    public decimal PrecoUnitario { get; set; }
    public bool VinculadoEstoque => ProdutoId.HasValue;
    public string TipoDescricao => VinculadoEstoque ? "Estoque" : "Manual";
}
