using ImperialColors.Application.DTOs;
using ImperialColors.Application.Helpers;
using ImperialColors.Domain.Enums;
using ImperialColors.UI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ImperialColors.UI.Views;

public partial class FechamentoVendaView : Window
{
    private readonly decimal _total;
    private FormaPagamento _formaSelecionada = FormaPagamento.Dinheiro;

    public CriarVendaDto? Pagamento { get; private set; }

    public FechamentoVendaView(decimal total)
    {
        InitializeComponent();
        _total = total;
        TxtTotal.Text = FormattingHelper.FormatarMoeda(total);

        for (var i = 1; i <= 12; i++)
            CmbParcelas.Items.Add($"{i}x");

        CmbParcelas.SelectedIndex = 0;
        SelecionarForma(FormaPagamento.Dinheiro, BtnDinheiro);
    }

    private void FormaPagamento_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton botao || botao.Tag is not string tag)
            return;

        if (!Enum.TryParse<FormaPagamento>(tag, out var forma))
            return;

        SelecionarForma(forma, botao);
    }

    private void SelecionarForma(FormaPagamento forma, ToggleButton botaoAtivo)
    {
        _formaSelecionada = forma;

        foreach (var child in GridFormasPagamento.Children.OfType<ToggleButton>())
            child.IsChecked = ReferenceEquals(child, botaoAtivo);

        PainelDinheiro.Visibility = PagamentoHelper.UsaTroco(forma) ? Visibility.Visible : Visibility.Collapsed;
        PainelParcelas.Visibility = PagamentoHelper.PermiteParcelamento(forma) ? Visibility.Visible : Visibility.Collapsed;
        PainelAutomatico.Visibility = PagamentoHelper.ValorPagoAutomatico(forma) ? Visibility.Visible : Visibility.Collapsed;

        if (PagamentoHelper.UsaTroco(forma))
        {
            if (string.IsNullOrWhiteSpace(TxtValorRecebido.Text))
                TxtValorRecebido.Text = FormattingHelper.FormatarMoedaEntrada(_total);
            AtualizarTroco();
        }
        else
        {
            TxtInfoAutomatico.Text =
                $"Valor pago automaticamente: {FormattingHelper.FormatarMoeda(_total)}";
        }
    }

    private void TxtValorRecebido_TextChanged(object sender, TextChangedEventArgs e) => AtualizarTroco();

    private void CmbParcelas_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void AtualizarTroco()
    {
        FormattingHelper.TryParseMoeda(TxtValorRecebido.Text, out var recebido);
        var troco = Math.Max(0, recebido - _total);
        TxtTroco.Text = FormattingHelper.FormatarMoeda(troco);
    }

    private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var parcelas = CmbParcelas.SelectedIndex >= 0 ? CmbParcelas.SelectedIndex + 1 : 1;
            decimal valorRecebido = _total;

            if (PagamentoHelper.UsaTroco(_formaSelecionada))
            {
                if (!FormattingHelper.TryParseMoeda(TxtValorRecebido.Text, out valorRecebido))
                {
                    MessageBox.Show("Informe um valor recebido válido.", "Validação",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            PagamentoHelper.ValidarPagamento(_formaSelecionada, _total, valorRecebido, parcelas);
            var (valorPago, troco, parcelasFinal) =
                PagamentoHelper.CalcularPagamento(_formaSelecionada, _total, valorRecebido, parcelas);

            Pagamento = new CriarVendaDto
            {
                FormaPagamento = _formaSelecionada,
                QuantidadeParcelas = parcelasFinal,
                ValorPago = valorPago,
                Troco = troco
            };

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
