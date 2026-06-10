using ImperialColors.Application.DTOs;
using ImperialColors.UI.Services;
using System.Windows;

namespace ImperialColors.UI.Views;

public partial class CupomView : Window
{
    private readonly IRelatorioService _relatorioService;
    private VendaDto? _venda;

    public CupomView(IRelatorioService relatorioService)
    {
        InitializeComponent();
        _relatorioService = relatorioService;
    }

    public void InicializarVenda(VendaDto venda)
    {
        _venda = venda;
        TxtNumeroVenda.Text = venda.NumeroVenda;
        TxtData.Text = venda.DataVenda.ToString("dd/MM/yyyy HH:mm");
        TxtCliente.Text = venda.ClienteNome ?? "Consumidor Final";
        TxtSubtotal.Text = venda.Subtotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        TxtDesconto.Text = venda.Desconto.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        TxtTotal.Text = venda.Total.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        LstItens.ItemsSource = venda.Itens;
    }

    private void BtnImprimir_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Controls.PrintDialog();
        if (dialog.ShowDialog() == true)
            dialog.PrintVisual(BorderCupom, $"Cupom {_venda?.NumeroVenda}");
    }

    private async void BtnSalvarPdf_Click(object sender, RoutedEventArgs e)
    {
        if (_venda is null) return;

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"Cupom_{_venda.NumeroVenda}",
            DefaultExt = ".pdf",
            Filter = "PDF|*.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                await _relatorioService.GerarCupomPdfAsync(_venda, saveDialog.FileName);
                MessageBox.Show($"PDF salvo em:\n{saveDialog.FileName}", "PDF Gerado",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar PDF: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnFechar_Click(object sender, RoutedEventArgs e) => Close();
}
