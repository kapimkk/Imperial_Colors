using ImperialColors.Application.DTOs;
using ImperialColors.Application.Extensions;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Infrastructure.Data;
using ImperialColors.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImperialColors.Application.Tests;

/// <summary>
/// Critérios:
/// 1) Venda mista (estoque + manual) grava faturamento e baixa só item vinculado.
/// 2) Estoque insuficiente provoca rollback completo.
/// </summary>
public class VendaExternaIntegrationTests
{
    private static bool TryConfigurar(out ServiceProvider provider)
    {
        provider = null!;
        if (!IntegrationTestGuard.TryObterConnectionString(out var cs))
            return false;

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddInfrastructure(cs);
        services.AddApplication();
        provider = services.BuildServiceProvider();
        return true;
    }

    [Fact]
    public async Task RegistrarVendaExternaMista_DeveBaixarEstoqueApenasDoProdutoVinculado()
    {
        if (!TryConfigurar(out var provider)) return;
        await using var scope = provider.CreateAsyncScope();

        var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();
        var vendaExternaService = scope.ServiceProvider.GetRequiredService<IVendaExternaService>();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var produto = await produtoService.CriarAsync(new CriarProdutoDto
        {
            Nome = $"Tinta Externa {sufixo}",
            CodigoInterno = $"EXT-{sufixo}",
            CodigoInternoDefinidoManualmente = true,
            CodigoBarras = $"789{sufixo}",
            PrecoVenda = 100m,
            QuantidadeEstoque = 10,
            EstoqueMinimo = 1
        });

        const decimal qtdBaixa = 2m;
        var venda = await vendaExternaService.RegistrarAsync(new RegistrarVendaExternaDto
        {
            Usuario = "Teste",
            Itens =
            [
                new RegistrarItemVendaExternaDto
                {
                    ProdutoId = produto.Id,
                    NomeProduto = produto.Nome,
                    CodigoBarras = produto.CodigoBarras,
                    Quantidade = qtdBaixa,
                    PrecoBase = produto.PrecoVenda,
                    PrecoUnitario = 95m
                },
                new RegistrarItemVendaExternaDto
                {
                    ProdutoId = null,
                    NomeProduto = "Cor personalizada avulsa",
                    Quantidade = 1,
                    PrecoBase = 0,
                    PrecoUnitario = 50m
                }
            ]
        });

        Assert.StartsWith("EXT-", venda.NumeroVendaExterna);
        Assert.Equal(2, venda.Itens.Count);
        Assert.Equal(240m, venda.Total);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        var prodBanco = await ctx.Set<Domain.Entities.Produto>().FirstAsync(p => p.Id == produto.Id);
        Assert.Equal(10m - qtdBaixa, prodBanco.QuantidadeEstoque);

        var movimentacoes = await ctx.Set<Domain.Entities.MovimentacaoEstoque>()
            .Where(m => m.VendaExternaId == venda.Id)
            .ToListAsync();
        Assert.Single(movimentacoes);
        Assert.Equal(qtdBaixa, movimentacoes[0].Quantidade);
    }

    [Fact]
    public async Task RegistrarVendaExterna_EstoqueInsuficiente_DeveFazerRollback()
    {
        if (!TryConfigurar(out var provider)) return;
        await using var scope = provider.CreateAsyncScope();

        var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();
        var vendaExternaService = scope.ServiceProvider.GetRequiredService<IVendaExternaService>();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var produto = await produtoService.CriarAsync(new CriarProdutoDto
        {
            Nome = $"Produto Rollback {sufixo}",
            CodigoInterno = $"RB-{sufixo}",
            CodigoInternoDefinidoManualmente = true,
            PrecoVenda = 50m,
            QuantidadeEstoque = 1,
            EstoqueMinimo = 0
        });

        var antes = await vendaExternaService.ObterTodosAsync();

        await Assert.ThrowsAsync<DomainException>(() => vendaExternaService.RegistrarAsync(new RegistrarVendaExternaDto
        {
            Itens =
            [
                new RegistrarItemVendaExternaDto
                {
                    ProdutoId = produto.Id,
                    NomeProduto = produto.Nome,
                    Quantidade = 5,
                    PrecoBase = produto.PrecoVenda,
                    PrecoUnitario = produto.PrecoVenda
                }
            ]
        }));

        var depois = await vendaExternaService.ObterTodosAsync();
        Assert.Equal(antes.Count(), depois.Count());

        await using var ctx = await contextFactory.CreateDbContextAsync();
        var prodBanco = await ctx.Set<Domain.Entities.Produto>().FirstAsync(p => p.Id == produto.Id);
        Assert.Equal(1m, prodBanco.QuantidadeEstoque);
    }
}
