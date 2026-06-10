using ImperialColors.Application.Configuration;
using ImperialColors.Application.DTOs;
using ImperialColors.Domain.Enums;
using ImperialColors.UI.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Xunit;

namespace ImperialColors.UI.Tests;

public class CupomPdfTests
{
    private static VendaDto CriarVendaExemplo()
    {
        return new VendaDto
        {
            Id = 1,
            NumeroVenda = "V-2026-0001",
            DataVenda = new DateTime(2026, 6, 10, 14, 30, 0),
            ClienteNome = "Cliente Teste",
            Subtotal = 59.80m,
            Desconto = 0m,
            Total = 59.80m,
            FormaPagamento = FormaPagamento.Pix,
            ValorPago = 59.80m,
            Troco = 0m,
            Itens =
            [
                new ItemVendaDto
                {
                    NomeProduto = "Tinta Acrilica Branca",
                    Quantidade = 2,
                    Unidade = "UN",
                    PrecoUnitario = 29.90m,
                    Subtotal = 59.80m
                }
            ]
        };
    }

    [Fact]
    public async Task GerarCupomPdf_DeveUsarCnpjDaConfiguracaoExterna()
    {
        var cnpjEsperado = "11.222.333/0001-44";
        var pastaTemp = Path.Combine(Path.GetTempPath(), "ImperialColorsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pastaTemp);

        var appsettingsPath = Path.Combine(pastaTemp, "appsettings.json");
        await File.WriteAllTextAsync(appsettingsPath,
            $$"""
            {
              "DadosEmpresa": {
                "NomeFantasia": "Imperial Colors Teste",
                "RazaoSocial": "Imperial Colors Teste LTDA",
                "Subtitulo": "Tintas",
                "CNPJ": "{{cnpjEsperado}}",
                "Endereco": "Rua Teste, 123 - Curitiba - PR",
                "Telefone": "(41) 98888-7777"
              },
              "Cupom": {
                "MensagemRodape": "Obrigado!"
              }
            }
            """);

        var pdfPath = Path.Combine(pastaTemp, "cupom-teste.pdf");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(pastaTemp)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = CriarServicosCupom(configuration);

        await using var provider = services.BuildServiceProvider();
        var relatorio = provider.GetRequiredService<IRelatorioService>();

        await relatorio.GerarCupomPdfAsync(CriarVendaExemplo(), pdfPath);

        Assert.True(File.Exists(pdfPath));

        var textoPdf = ExtrairTextoPdf(pdfPath);

        Assert.Contains(cnpjEsperado, textoPdf);
        Assert.Contains("Forma:", textoPdf);
        Assert.Contains("Pix", textoPdf);
        Assert.DoesNotContain("Forma:                             ", textoPdf);
    }

    [Fact]
    public async Task GerarCupomPdf_AlterarCnpjNoAppsettings_DeveRefletirSemRecompilar()
    {
        var pastaTemp = Path.Combine(Path.GetTempPath(), "ImperialColorsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pastaTemp);

        var appsettingsPath = Path.Combine(pastaTemp, "appsettings.json");
        await File.WriteAllTextAsync(appsettingsPath,
            """
            {
              "DadosEmpresa": {
                "NomeFantasia": "Imperial Colors",
                "CNPJ": "00.000.000/0001-00",
                "Endereco": "Endereco Original"
              }
            }
            """);

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(pastaTemp)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var services = CriarServicosCupom(configuration);
        await using var provider = services.BuildServiceProvider();
        var relatorio = provider.GetRequiredService<IRelatorioService>();

        var pdf1 = Path.Combine(pastaTemp, "cupom-1.pdf");
        await relatorio.GerarCupomPdfAsync(CriarVendaExemplo(), pdf1);
        Assert.Contains("00.000.000/0001-00", ExtrairTextoPdf(pdf1));

        await File.WriteAllTextAsync(appsettingsPath,
            """
            {
              "DadosEmpresa": {
                "NomeFantasia": "Imperial Colors",
                "CNPJ": "99.888.777/0001-55",
                "Endereco": "Endereco Atualizado"
              }
            }
            """);

        configuration = new ConfigurationBuilder()
            .SetBasePath(pastaTemp)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        services = CriarServicosCupom(configuration);
        await using var providerAtualizado = services.BuildServiceProvider();
        relatorio = providerAtualizado.GetRequiredService<IRelatorioService>();

        var pdf2 = Path.Combine(pastaTemp, "cupom-2.pdf");
        await relatorio.GerarCupomPdfAsync(CriarVendaExemplo(), pdf2);

        var textoAtualizado = ExtrairTextoPdf(pdf2);
        Assert.Contains("99.888.777/0001-55", textoAtualizado);
        Assert.Contains("Endereco Atualizado", textoAtualizado);
    }

    private static ServiceCollection CriarServicosCupom(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddOptions<EmpresaConfig>()
            .Bind(configuration.GetSection(EmpresaConfig.Secao));
        services.PostConfigure<EmpresaConfig>(EmpresaConfigEnvironmentOverrides.Aplicar);
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<IRelatorioService, RelatorioService>();
        return services;
    }

    private static string ExtrairTextoPdf(string caminho)
    {
        using var reader = new PdfReader(caminho);
        using var pdf = new PdfDocument(reader);
        var texto = new System.Text.StringBuilder();

        for (var i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            var pagina = pdf.GetPage(i);
            texto.Append(PdfTextExtractor.GetTextFromPage(pagina, new LocationTextExtractionStrategy()));
        }

        return texto.ToString();
    }
}
