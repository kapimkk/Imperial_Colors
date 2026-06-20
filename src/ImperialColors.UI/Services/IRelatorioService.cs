using ImperialColors.Application.DTOs;

namespace ImperialColors.UI.Services;

public interface IRelatorioService
{
    Task GerarCupomPdfAsync(VendaDto venda, string caminhoArquivo);
    Task GerarRelatorioVendasPdfAsync(IEnumerable<VendaDto> vendas, DateTime inicio, DateTime fim, string caminhoArquivo);
    Task GerarRelatorioEstoquePdfAsync(IEnumerable<ProdutoDto> produtos, string caminhoArquivo);
    Task GerarRelatorioVendasExcelAsync(IEnumerable<VendaDto> vendas, DateTime inicio, DateTime fim, string caminhoArquivo);
    Task GerarRelatorioEstoqueExcelAsync(IEnumerable<ProdutoDto> produtos, string caminhoArquivo);
    Task GerarTabelaPrecosPdfAsync(IEnumerable<ProdutoDto> produtos, string titulo, string subtitulo, decimal acrescimoPercentual, string caminhoArquivo);
    Task GerarTabelaPrecosExcelAsync(IEnumerable<ProdutoDto> produtos, string titulo, decimal acrescimoPercentual, string caminhoArquivo);
}
