using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Enums;
using System.Globalization;
using System.Windows;

namespace ImperialColors.UI.Views;

public partial class MovimentacaoEstoqueView : Window
{
    private readonly IProdutoService _produtoService;
    private ProdutoDto? _produto;

    public MovimentacaoEstoqueView(IProdutoService produtoService)
    {
        InitializeComponent();
        _produtoService = produtoService;
    }

    public void InicializarProduto(ProdutoDto produto)
    {
        _produto = produto;
        TxtNomeProduto.Text = $"{produto.CodigoInterno} - {produto.Nome}";
        TxtEstoqueAtual.Text = $"{produto.QuantidadeEstoque:G} {produto.Unidade}";
    }

    private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        if (_produto is null) return;
        if (string.IsNullOrWhiteSpace(TxtMotivo.Text))
        {
            MessageBox.Show("Motivo é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtQuantidade.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal quantidade) || quantidade <= 0)
        {
            MessageBox.Show("Quantidade inválida.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var tipo = (CmbTipo.SelectedIndex) switch
            {
                0 => TipoMovimentacao.Entrada,
                1 => TipoMovimentacao.Saida,
                _ => TipoMovimentacao.Ajuste
            };

            await _produtoService.RegistrarMovimentacaoAsync(new MovimentacaoEstoqueDto
            {
                ProdutoId = _produto.Id,
                Tipo = tipo,
                Quantidade = quantidade,
                Motivo = TxtMotivo.Text.Trim(),
                Usuario = "Administrador"
            });

            MessageBox.Show("Movimentação registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
