using ImperialColors.Application.Helpers;
using ImperialColors.Application.Interfaces;
using ImperialColors.UI.Helpers;
using ImperialColors.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace ImperialColors.UI.Views;

public partial class RelatoriosView : UserControl
{
    private readonly IServiceProvider _serviceProvider;

    public RelatoriosView(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        DpInicio.SelectedDate = DateTime.Today.AddDays(-30);
        DpFim.SelectedDate = DateTime.Today;
        Loaded += (_, _) =>
        {
            DatePickerSyncHelper.SincronizarTexto(DpInicio);
            DatePickerSyncHelper.SincronizarTexto(DpFim);
        };
    }

    private (DateTime inicio, DateTime fim) ObterPeriodo()
        => (DpInicio.SelectedDate ?? DateTime.Today.AddDays(-30),
            DpFim.SelectedDate ?? DateTime.Today);

    private static bool TentarObterCaminhoSalvar(string nomePadrao, bool excel, out string caminho)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = nomePadrao,
            DefaultExt = excel ? ".xlsx" : ".pdf",
            Filter = excel ? "Excel|*.xlsx" : "PDF|*.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            caminho = saveDialog.FileName;
            return true;
        }

        caminho = string.Empty;
        return false;
    }

    private static void NotificarSucesso(string caminho)
        => MessageBox.Show($"Arquivo gerado com sucesso:\n{caminho}", "Relatório", MessageBoxButton.OK, MessageBoxImage.Information);

    private static void NotificarErro(Exception ex)
        => MessageBox.Show($"Erro ao gerar relatório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

    private async void BtnGerarVendas_Click(object sender, RoutedEventArgs e)
    {
        var (inicio, fim) = ObterPeriodo();
        var excel = RbVendasExcel.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"Vendas_{inicio:yyyyMMdd}_{fim:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var vendas = await _serviceProvider.GetRequiredService<IVendaService>()
                .ObterPorPeriodoAsync(inicio, fim.AddDays(1).AddSeconds(-1));
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();

            if (excel)
                await relatorio.GerarRelatorioVendasExcelAsync(vendas, inicio, fim, caminho);
            else
                await relatorio.GerarRelatorioVendasPdfAsync(vendas, inicio, fim, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }

    private async void BtnGerarEstoque_Click(object sender, RoutedEventArgs e)
    {
        var excel = RbEstoqueExcel?.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"Estoque_{DateTime.Today:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterTodosAsync();
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();

            if (excel)
                await relatorio.GerarRelatorioEstoqueExcelAsync(produtos, caminho);
            else
                await relatorio.GerarRelatorioEstoquePdfAsync(produtos, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }

    private async void BtnGerarEstoqueBaixo_Click(object sender, RoutedEventArgs e)
    {
        var excel = RbEstoqueBaixoExcel?.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"EstoqueBaixo_{DateTime.Today:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterComEstoqueBaixoAsync();
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();

            if (excel)
                await relatorio.GerarRelatorioEstoqueExcelAsync(produtos, caminho);
            else
                await relatorio.GerarRelatorioEstoquePdfAsync(produtos, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }

    private async void BtnGerarSemEstoque_Click(object sender, RoutedEventArgs e)
    {
        var excel = RbSemEstoqueExcel?.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"SemEstoque_{DateTime.Today:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterSemEstoqueAsync();
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();

            if (excel)
                await relatorio.GerarRelatorioEstoqueExcelAsync(produtos, caminho);
            else
                await relatorio.GerarRelatorioEstoquePdfAsync(produtos, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }

    private async void BtnGerarTabelaLoja_Click(object sender, RoutedEventArgs e)
    {
        var excel = RbTabelaLojaExcel?.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"TabelaVendasLoja_{DateTime.Today:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterTodosAsync();
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();
            const string titulo = "Tabela de Vendas da Loja";
            var subtitulo = $"Catalogo de precos de balcao — Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            if (excel)
                await relatorio.GerarTabelaPrecosExcelAsync(produtos, titulo, 0m, caminho);
            else
                await relatorio.GerarTabelaPrecosPdfAsync(produtos, titulo, subtitulo, 0m, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }

    private async void BtnGerarTabelaPintor_Click(object sender, RoutedEventArgs e)
    {
        var excel = RbTabelaPintorExcel?.IsChecked == true;
        if (!TentarObterCaminhoSalvar($"TabelaPintor_{DateTime.Today:yyyyMMdd}", excel, out var caminho))
            return;

        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterTodosAsync();
            var relatorio = _serviceProvider.GetRequiredService<IRelatorioService>();
            const string titulo = "Tabela do Pintor";
            var subtitulo = $"Precos com acrescimo de {TabelaPrecosHelper.AcrescimoTabelaPintorPercentual:N0}% para parceiros — Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            if (excel)
                await relatorio.GerarTabelaPrecosExcelAsync(produtos, titulo, TabelaPrecosHelper.AcrescimoTabelaPintorPercentual, caminho);
            else
                await relatorio.GerarTabelaPrecosPdfAsync(produtos, titulo, subtitulo, TabelaPrecosHelper.AcrescimoTabelaPintorPercentual, caminho);

            NotificarSucesso(caminho);
        }
        catch (Exception ex) { NotificarErro(ex); }
    }
}
