using ImperialColors.Application.DTOs;
using ImperialColors.Application.Extensions;
using ImperialColors.Application.Interfaces;
using ImperialColors.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImperialColors.Application.Tests;

public class ProdutoCodigoInternoIntegrationTests
{
    [Fact]
    public async Task CriarTresProdutosSeguidos_DeveGerarCodigosSequenciaisUnicos()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env"));
        if (!File.Exists(envPath))
            return;

        DotNetEnv.Env.Load(envPath);

        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
            return;

        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var db = Environment.GetEnvironmentVariable("DB_NAME") ?? "imperial_colors";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var ssl = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "Prefer";

        var cs = $"Host={host};Port={port};Database={db};Username={user};Password={password};SSL Mode={ssl};Trust Server Certificate=true;";

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddInfrastructure(cs);
        services.AddApplication();

        await using var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();
        var categoriaService = scope.ServiceProvider.GetRequiredService<ICategoriaService>();
        var marcaService = scope.ServiceProvider.GetRequiredService<IMarcaService>();

        var categorias = (await categoriaService.ObterTodosAsync()).ToList();
        var marcas = (await marcaService.ObterTodosAsync()).ToList();

        if (categorias.Count == 0 || marcas.Count == 0)
            return;

        var maiorAntes = (await produtoService.ObterTodosAsync())
            .Select(p => p.CodigoInterno)
            .Where(c => c.StartsWith('P') && int.TryParse(c[1..], out _))
            .Select(c => int.Parse(c[1..]))
            .DefaultIfEmpty(0)
            .Max();

        var codigosGerados = new List<string>();

        for (var i = 0; i < 3; i++)
        {
            var codigoSugerido = await produtoService.GerarProximoCodigoInternoAsync();
            var criado = await produtoService.CriarAsync(new CriarProdutoDto
            {
                CodigoInterno = codigoSugerido,
                CodigoInternoDefinidoManualmente = false,
                Nome = $"Produto Stress {Guid.NewGuid():N}",
                CategoriaId = categorias[0].Id,
                MarcaId = marcas[0].Id,
                Custo = 10m,
                PrecoVenda = 20m,
                QuantidadeEstoque = 0,
                EstoqueMinimo = 0,
                Unidade = "UN"
            });

            codigosGerados.Add(criado.CodigoInterno);
        }

        Assert.Equal(3, codigosGerados.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(codigosGerados, c => Assert.Matches("^P[0-9]{5}$", c));
        Assert.All(codigosGerados, c => Assert.True(int.Parse(c[1..]) > maiorAntes || maiorAntes == 0));
    }
}
