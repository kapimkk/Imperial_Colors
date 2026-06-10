using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Interfaces;

namespace ImperialColors.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IClienteRepository _clienteRepository;

    public DashboardService(
        IVendaRepository vendaRepository,
        IProdutoRepository produtoRepository,
        IClienteRepository clienteRepository)
    {
        _vendaRepository = vendaRepository;
        _produtoRepository = produtoRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<DashboardDto> ObterDadosDashboardAsync()
    {
        var hoje = DateTime.Today;

        var totalVendasHoje = await _vendaRepository.ObterTotalVendasDiaAsync(hoje);
        var totalVendasMes = await _vendaRepository.ObterTotalVendasMesAsync(hoje.Year, hoje.Month);
        var totalClientes = await _clienteRepository.ContarAsync();
        var totalProdutos = await _produtoRepository.ContarAsync();

        var produtosEstoqueBaixo = await _produtoRepository.ObterComEstoqueBaixoAsync();
        var produtosSemEstoque = await _produtoRepository.ObterSemEstoqueAsync();

        var vendas = await _vendaRepository.ObterPorPeriodoAsync(hoje, hoje.AddDays(1).AddSeconds(-1));

        return new DashboardDto
        {
            TotalVendasHoje = totalVendasHoje,
            TotalVendasMes = totalVendasMes,
            QuantidadeVendasHoje = vendas.Count(),
            ProdutosEstoqueBaixo = produtosEstoqueBaixo.Count(),
            ProdutosSemEstoque = produtosSemEstoque.Count(),
            TotalClientes = totalClientes,
            TotalProdutos = totalProdutos,
            ProdutosBaixoEstoque = produtosEstoqueBaixo
                .Take(10)
                .Select(p => new ProdutoBaixoEstoqueDto
                {
                    Nome = p.Nome,
                    Quantidade = p.QuantidadeEstoque,
                    EstoqueMinimo = p.EstoqueMinimo,
                    Unidade = p.Unidade
                }).ToList()
        };
    }
}
