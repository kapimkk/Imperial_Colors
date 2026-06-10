using ImperialColors.Application.Interfaces;
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
    }

    private (DateTime inicio, DateTime fim) ObterPeriodo()
        => (DpInicio.SelectedDate ?? DateTime.Today.AddDays(-30),
            DpFim.SelectedDate ?? DateTime.Today);

    private async void BtnVendasPdf_Click(object sender, RoutedEventArgs e)
    {
        var (inicio, fim) = ObterPeriodo();
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"Vendas_{inicio:yyyyMMdd}_{fim:yyyyMMdd}",
            DefaultExt = ".pdf", Filter = "PDF|*.pdf"
        };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var vendas = await _serviceProvider.GetRequiredService<IVendaService>()
                .ObterPorPeriodoAsync(inicio, fim.AddDays(1).AddSeconds(-1));
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioVendasPdfAsync(vendas, inicio, fim, saveDialog.FileName);
            MessageBox.Show($"PDF gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void BtnVendasExcel_Click(object sender, RoutedEventArgs e)
    {
        var (inicio, fim) = ObterPeriodo();
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"Vendas_{inicio:yyyyMMdd}_{fim:yyyyMMdd}",
            DefaultExt = ".xlsx", Filter = "Excel|*.xlsx"
        };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var vendas = await _serviceProvider.GetRequiredService<IVendaService>()
                .ObterPorPeriodoAsync(inicio, fim.AddDays(1).AddSeconds(-1));
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioVendasExcelAsync(vendas, inicio, fim, saveDialog.FileName);
            MessageBox.Show($"Excel gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void BtnEstoquePdf_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        { FileName = "Estoque", DefaultExt = ".pdf", Filter = "PDF|*.pdf" };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterTodosAsync();
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioEstoquePdfAsync(produtos, saveDialog.FileName);
            MessageBox.Show($"PDF gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void BtnEstoqueExcel_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        { FileName = "Estoque", DefaultExt = ".xlsx", Filter = "Excel|*.xlsx" };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterTodosAsync();
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioEstoqueExcelAsync(produtos, saveDialog.FileName);
            MessageBox.Show($"Excel gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void BtnEstoqueBaixoPdf_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        { FileName = "EstoqueBaixo", DefaultExt = ".pdf", Filter = "PDF|*.pdf" };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterComEstoqueBaixoAsync();
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioEstoquePdfAsync(produtos, saveDialog.FileName);
            MessageBox.Show($"PDF gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void BtnSemEstoquePdf_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        { FileName = "SemEstoque", DefaultExt = ".pdf", Filter = "PDF|*.pdf" };
        if (saveDialog.ShowDialog() != true) return;
        try
        {
            var produtos = await _serviceProvider.GetRequiredService<IProdutoService>().ObterSemEstoqueAsync();
            await _serviceProvider.GetRequiredService<IRelatorioService>()
                .GerarRelatorioEstoquePdfAsync(produtos, saveDialog.FileName);
            MessageBox.Show($"PDF gerado: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
