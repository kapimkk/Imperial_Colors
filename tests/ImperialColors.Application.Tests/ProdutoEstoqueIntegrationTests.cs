using ImperialColors.Application.DTOs;
using ImperialColors.Application.Extensions;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using ImperialColors.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImperialColors.Application.Tests;

/// <summary>
/// Homologação do módulo de estoque: busca paginada, buscas alternadas e soft delete.
/// Requer .env com PostgreSQL acessível; pula silenciosamente se indisponível.
/// </summary>
public class ProdutoEstoqueIntegrationTests
{
    private static bool TryCarregarServicos(out ServiceProvider provider, out IProdutoService produtoService, out IDbContextFactory<AppDbContext> contextFactory)
    {
        provider = null!;
        produtoService = null!;
        contextFactory = null!;

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env"));
        if (!File.Exists(envPath))
            return false;

        DotNetEnv.Env.Load(envPath);

        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "imperial_colors";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var ssl = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "Prefer";

        var cs = $"Host={host};Port={port};Database={dbName};Username={user};Password={password};SSL Mode={ssl};Trust Server Certificate=true;";

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddInfrastructure(cs);
        services.AddApplication();

        provider = services.BuildServiceProvider();
        produtoService = provider.GetRequiredService<IProdutoService>();
        contextFactory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        return true;
    }

    [Fact]
    public async Task Estoque_BuscaAlternadaEExclusao_DevePersistirSemErroDeContexto()
    {
        if (!TryCarregarServicos(out var provider, out var produtoService, out var contextFactory))
            return;

        await using (provider)
        {
            var categoriaService = provider.GetRequiredService<ICategoriaService>();
            var marcaService = provider.GetRequiredService<IMarcaService>();
            var categoriaRepo = provider.GetRequiredService<IRepository<Domain.Entities.Categoria>>();
            var marcaRepo = provider.GetRequiredService<IRepository<Domain.Entities.Marca>>();

            var sufixo = Guid.NewGuid().ToString("N")[..8];
            var termoBase = $"Homolog{sufixo}";

            var categoria = await categoriaRepo.AdicionarAsync(new Domain.Entities.Categoria
            {
                Nome = $"Cat Homolog {sufixo}",
                Ativo = true
            });

            var marca = await marcaRepo.AdicionarAsync(new Domain.Entities.Marca
            {
                Nome = $"Marca Homolog {sufixo}",
                Ativo = true
            });

            var idsCriados = new List<int>();
            var nomes = new[]
            {
                $"{termoBase} Alpha Tinta",
                $"{termoBase} Beta Verniz",
                $"{termoBase} Gamma Esmalte"
            };

            foreach (var nome in nomes)
            {
                var codigo = await produtoService.GerarProximoCodigoInternoAsync();
                var criado = await produtoService.CriarAsync(new CriarProdutoDto
                {
                    CodigoInterno = codigo,
                    Nome = nome,
                    CategoriaId = categoria.Id,
                    MarcaId = marca.Id,
                    Custo = 15m,
                    PrecoVenda = 29.90m,
                    QuantidadeEstoque = 10,
                    EstoqueMinimo = 2,
                    Unidade = "UN"
                });
                idsCriados.Add(criado.Id);
            }

            var termosBusca = new[] { termoBase, "Alpha", "Beta", "Gamma", termoBase, "Alpha" };
            foreach (var termo in termosBusca)
            {
                var pagina = await produtoService.ObterPaginadoAsync(1, 50, termo);
                Assert.Contains(pagina.Itens, p => p.Nome.Contains(termoBase, StringComparison.OrdinalIgnoreCase));
            }

            var idExcluir = idsCriados[0];
            await produtoService.RemoverAsync(idExcluir);

            var aposExclusao = await produtoService.ObterPaginadoAsync(1, 50, termoBase);
            Assert.DoesNotContain(aposExclusao.Itens, p => p.Id == idExcluir);
            Assert.Equal(2, aposExclusao.Itens.Count(p => p.Nome.Contains(termoBase, StringComparison.OrdinalIgnoreCase)));

            await using var db = await contextFactory.CreateDbContextAsync();
            var noBanco = await db.Produtos.IgnoreQueryFilters()
                .FirstAsync(p => p.Id == idExcluir);
            Assert.False(noBanco.Ativo);

            var obterPorId = await produtoService.ObterPorIdAsync(idExcluir);
            Assert.Null(obterPorId);
        }
    }
}
