using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Interfaces;

namespace ImperialColors.Application.Services;

public class RelatorioAnalyticsService : IRelatorioAnalyticsService
{
    private readonly IRelatorioAnalyticsRepository _repository;

    public RelatorioAnalyticsService(IRelatorioAnalyticsRepository repository)
        => _repository = repository;

    public async Task<IReadOnlyList<LinhaRelatorioVendaExternaDto>> ObterLinhasVendasExternasAsync(
        DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        var linhas = await _repository.ObterLinhasVendasExternasPorPeriodoAsync(inicio, fim, cancellationToken);
        return linhas.Select(l => new LinhaRelatorioVendaExternaDto
        {
            DataVenda = l.DataVenda,
            CodigoVenda = l.CodigoVenda,
            ProdutoItem = l.ProdutoItem,
            QuantidadeVendida = l.QuantidadeVendida,
            ValorUnitario = l.ValorUnitario,
            ValorTotal = l.ValorTotal
        }).ToList();
    }

    public async Task<IReadOnlyList<ProdutoRankingDto>> ObterRankingProdutosAsync(
        DateTime inicio, DateTime fim, TipoAnaliseGiroProduto tipo, CancellationToken cancellationToken = default)
    {
        var ranking = tipo switch
        {
            TipoAnaliseGiroProduto.MaisVendidos =>
                await _repository.ObterProdutosMaisVendidosAsync(inicio, fim, cancellationToken),
            TipoAnaliseGiroProduto.MenosVendidos =>
                await _repository.ObterProdutosMenosVendidosAsync(inicio, fim, cancellationToken),
            _ => Array.Empty<Domain.ReadModels.ProdutoRankingResumo>()
        };

        return ranking
            .Select((r, index) => new ProdutoRankingDto
            {
                Posicao = index + 1,
                CodigoInterno = r.CodigoInterno,
                NomeProduto = r.NomeProduto,
                QuantidadeTotal = r.QuantidadeTotal,
                FaturamentoGerado = r.FaturamentoGerado
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProdutoEncalhadoDto>> ObterProdutosEncalhadosAsync(
        DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ObterProdutosNuncaVendidosAsync(inicio, fim, cancellationToken);
        return itens.Select(i => new ProdutoEncalhadoDto
        {
            CodigoInterno = i.CodigoInterno,
            NomeProduto = i.NomeProduto,
            EstoqueAtual = i.EstoqueAtual,
            ValorTotalParado = i.ValorTotalParado
        }).ToList();
    }
}
