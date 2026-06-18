using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Exceptions;
using ImperialColors.UI.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ImperialColors.UI.Views;

public partial class ProdutoFormView : Window
{
    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IMarcaService _marcaService;
    private int? _produtoId;
    private bool _codigoDefinidoManualmente;
    private string? _ultimoCodigoGeradoAutomaticamente;
    private bool _ignorarAlteracaoCodigoInterno;
    private bool _modoCustoTotal;
    private bool _suprimirAtualizacaoCusto;

    public ProdutoFormView(
        IProdutoService produtoService,
        ICategoriaService categoriaService,
        IMarcaService marcaService)
    {
        InitializeComponent();
        ModalWindowHelper.AplicarEstiloModerno(this);
        _produtoService = produtoService;
        _categoriaService = categoriaService;
        _marcaService = marcaService;
        Loaded += async (_, _) => await CarregarComboBoxesAsync();
    }

    private async Task CarregarComboBoxesAsync(int? categoriaId = null, int? marcaId = null)
    {
        var categorias = (await _categoriaService.ObterTodosAsync()).ToList();
        var marcas = (await _marcaService.ObterTodosAsync()).ToList();

        CmbCategoria.ItemsSource = categorias;
        CmbMarca.ItemsSource = marcas;

        if (categoriaId.HasValue && categoriaId > 0)
            CmbCategoria.SelectedValue = categoriaId;
        else if (categorias.Count > 0)
            CmbCategoria.SelectedIndex = 0;

        if (marcaId.HasValue && marcaId > 0)
            CmbMarca.SelectedValue = marcaId;
        else if (marcas.Count > 0)
            CmbMarca.SelectedIndex = 0;
    }

    public void InicializarNovo()
    {
        TxtTitulo.Text = "Novo Produto";
        _produtoId = null;
        _codigoDefinidoManualmente = false;
        _ultimoCodigoGeradoAutomaticamente = null;
        LimparErroValidacao();
        ChkCustoTotal.IsChecked = false;
        TxtCustoTotal.Text = string.Empty;
        PainelCustoTotal.Visibility = Visibility.Collapsed;
        TxtCusto.IsReadOnly = false;
        TxtCusto.Text = string.Empty;
        TxtPrecoVenda.Text = FormattingHelper.FormatarMoedaEntrada(0m);
        _ = GerarCodigoAsync();
    }

    public void InicializarEdicao(ProdutoDto produto)
    {
        TxtTitulo.Text = "Editar Produto";
        _produtoId = produto.Id;
        _codigoDefinidoManualmente = true;
        LimparErroValidacao();
        ChkCustoTotal.IsChecked = false;
        TxtCustoTotal.Text = string.Empty;
        PainelCustoTotal.Visibility = Visibility.Collapsed;
        TxtCusto.IsReadOnly = false;
        DefinirCodigoInternoSemMarcarManual(produto.CodigoInterno);
        TxtCodigoBarras.Text = produto.CodigoBarras ?? "";
        TxtNome.Text = produto.Nome;
        TxtQuantidade.Text = produto.QuantidadeEstoque.ToString(
            produto.QuantidadeEstoque % 1m == 0m ? "N0" : "N1",
            FormattingHelper.CulturaPtBr);
        TxtEstoqueMinimo.Text = produto.EstoqueMinimo.ToString(
            produto.EstoqueMinimo % 1m == 0m ? "N0" : "N1",
            FormattingHelper.CulturaPtBr);
        TxtCusto.Text = FormattingHelper.FormatarMoedaEntrada(produto.Custo);
        TxtPrecoVenda.Text = FormattingHelper.FormatarMoedaEntrada(produto.PrecoVenda);
        TxtObservacoes.Text = produto.Observacoes ?? "";

        foreach (ComboBoxItem item in CmbUnidade.Items)
            if (item.Content?.ToString() == produto.Unidade) { CmbUnidade.SelectedItem = item; break; }

        _ = CarregarComboBoxesAsync(produto.CategoriaId, produto.MarcaId);
    }

    private async Task GerarCodigoAsync()
    {
        var codigo = await _produtoService.GerarProximoCodigoInternoAsync();
        _codigoDefinidoManualmente = false;
        DefinirCodigoInternoSemMarcarManual(codigo);
        _ultimoCodigoGeradoAutomaticamente = codigo;
    }

    private void DefinirCodigoInternoSemMarcarManual(string codigo)
    {
        _ignorarAlteracaoCodigoInterno = true;
        TxtCodigoInterno.Text = codigo;
        _ignorarAlteracaoCodigoInterno = false;
    }

    private void TxtCodigoInterno_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_ignorarAlteracaoCodigoInterno || _produtoId.HasValue)
            return;

        var textoAtual = TxtCodigoInterno.Text.Trim();
        _codigoDefinidoManualmente = !string.Equals(
            textoAtual,
            _ultimoCodigoGeradoAutomaticamente,
            StringComparison.OrdinalIgnoreCase);
    }

    private void BtnGerarCodigo_Click(object sender, RoutedEventArgs e) => _ = GerarCodigoAsync();

    private void ChkCustoTotal_Changed(object sender, RoutedEventArgs e)
    {
        _modoCustoTotal = ChkCustoTotal.IsChecked == true;
        PainelCustoTotal.Visibility = _modoCustoTotal ? Visibility.Visible : Visibility.Collapsed;
        TxtCusto.IsReadOnly = _modoCustoTotal;

        if (_modoCustoTotal)
            CalcularCustoUnitarioPeloTotal();
        else
            TxtCustoTotal.Text = string.Empty;
    }

    private void TxtQuantidade_TextChanged(object sender, TextChangedEventArgs e)
        => CalcularCustoUnitarioPeloTotal();

    private void TxtCustoTotal_TextChanged(object sender, TextChangedEventArgs e)
        => CalcularCustoUnitarioPeloTotal();

    private void TxtCusto_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suprimirAtualizacaoCusto || _modoCustoTotal)
            return;
    }

    private void CalcularCustoUnitarioPeloTotal()
    {
        if (!_modoCustoTotal)
            return;

        if (!FormattingHelper.TryParseQuantidade(TxtQuantidade.Text, out var quantidade) || quantidade <= 0)
        {
            DefinirCustoCalculado(null);
            return;
        }

        if (!FormattingHelper.TryParseMoeda(TxtCustoTotal.Text, out var total) || total <= 0)
        {
            DefinirCustoCalculado(null);
            return;
        }

        DefinirCustoCalculado(total / quantidade);
    }

    private void DefinirCustoCalculado(decimal? custoUnitario)
    {
        _suprimirAtualizacaoCusto = true;
        TxtCusto.Text = custoUnitario.HasValue
            ? FormattingHelper.FormatarMoedaEntrada(custoUnitario.Value)
            : string.Empty;
        _suprimirAtualizacaoCusto = false;
    }

    private async void BtnNovaCategoria_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NomeRapidoDialogView("Nova Categoria", "Nome da Categoria *");
        if (ModalWindowHelper.ExibirDialogo(dialog, this) != true || string.IsNullOrWhiteSpace(dialog.NomeInformado))
            return;

        try
        {
            var criada = await _categoriaService.CriarAsync(dialog.NomeInformado);
            await CarregarComboBoxesAsync(criada.Id, ObterIdSelecionado(CmbMarca));
            LimparErroValidacao();
        }
        catch (Exception ex)
        {
            ExibirErroValidacao(ExceptionMessageHelper.ObterMensagemAmigavel(ex));
        }
    }

    private async void BtnNovaMarca_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NomeRapidoDialogView("Nova Marca", "Nome da Marca *");
        if (ModalWindowHelper.ExibirDialogo(dialog, this) != true || string.IsNullOrWhiteSpace(dialog.NomeInformado))
            return;

        try
        {
            var criada = await _marcaService.CriarAsync(dialog.NomeInformado);
            await CarregarComboBoxesAsync(ObterIdSelecionado(CmbCategoria), criada.Id);
            LimparErroValidacao();
        }
        catch (Exception ex)
        {
            ExibirErroValidacao(ExceptionMessageHelper.ObterMensagemAmigavel(ex));
        }
    }

    private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        LimparErroValidacao();

        if (!ValidarFormulario())
            return;

        try
        {
            FormattingHelper.TryParseMoedaOpcional(TxtCusto.Text, out decimal? custo);
            FormattingHelper.TryParseMoeda(TxtPrecoVenda.Text, out decimal preco);
            FormattingHelper.TryParseQuantidade(TxtQuantidade.Text, out decimal quantidade);
            FormattingHelper.TryParseQuantidade(TxtEstoqueMinimo.Text, out decimal estoqueMin);

            var categoriaId = ObterIdSelecionado(CmbCategoria);
            var marcaId = ObterIdSelecionado(CmbMarca);
            var unidade = (CmbUnidade.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "UN";

            BtnSalvar.IsEnabled = false;
            BtnSalvar.Content = "Salvando...";

            if (_produtoId.HasValue)
            {
                await _produtoService.AtualizarAsync(_produtoId.Value, new AtualizarProdutoDto
                {
                    Id = _produtoId.Value,
                    CodigoInterno = TxtCodigoInterno.Text.Trim(),
                    CodigoBarras = string.IsNullOrWhiteSpace(TxtCodigoBarras.Text) ? null : TxtCodigoBarras.Text.Trim(),
                    Nome = TxtNome.Text.Trim(),
                    CategoriaId = categoriaId,
                    MarcaId = marcaId,
                    QuantidadeEstoque = quantidade,
                    EstoqueMinimo = estoqueMin,
                    Unidade = unidade,
                    Custo = custo,
                    PrecoVenda = preco,
                    Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? null : TxtObservacoes.Text.Trim()
                });
            }
            else
            {
                await _produtoService.CriarAsync(new CriarProdutoDto
                {
                    CodigoInterno = TxtCodigoInterno.Text.Trim(),
                    CodigoInternoDefinidoManualmente = _codigoDefinidoManualmente,
                    CodigoBarras = string.IsNullOrWhiteSpace(TxtCodigoBarras.Text) ? null : TxtCodigoBarras.Text.Trim(),
                    Nome = TxtNome.Text.Trim(),
                    CategoriaId = categoriaId,
                    MarcaId = marcaId,
                    QuantidadeEstoque = quantidade,
                    EstoqueMinimo = estoqueMin,
                    Unidade = unidade,
                    Custo = custo,
                    PrecoVenda = preco,
                    Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? null : TxtObservacoes.Text.Trim()
                });
            }

            DialogResult = true;
            Close();
        }
        catch (DomainException ex)
        {
            ExibirErroValidacao(ex.Message);
        }
        catch (Exception ex)
        {
            ExibirErroValidacao(ExceptionMessageHelper.ObterMensagemAmigavel(ex));
        }
        finally
        {
            BtnSalvar.IsEnabled = true;
            BtnSalvar.Content = "Salvar Produto";
        }
    }

    private bool ValidarFormulario()
    {
        if (string.IsNullOrWhiteSpace(TxtCodigoInterno.Text))
        {
            ExibirErroValidacao("Código interno é obrigatório. Aguarde a geração automática ou clique em Gerar.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(TxtNome.Text))
        {
            ExibirErroValidacao("Nome do produto é obrigatório.");
            return false;
        }

        var categoriaId = ObterIdSelecionado(CmbCategoria);
        var marcaId = ObterIdSelecionado(CmbMarca);
        if (!categoriaId.HasValue || !marcaId.HasValue)
        {
            ExibirErroValidacao("Selecione uma Categoria e uma Marca válidas.");
            return false;
        }

        if (!FormattingHelper.TryParseMoedaOpcional(TxtCusto.Text, out decimal? custo))
        {
            ExibirErroValidacao("Preço de custo inválido.");
            return false;
        }

        if (_modoCustoTotal)
        {
            if (!FormattingHelper.TryParseMoeda(TxtCustoTotal.Text, out var totalCompra) || totalCompra <= 0)
            {
                ExibirErroValidacao("Informe o valor total da compra.");
                return false;
            }

            if (!FormattingHelper.TryParseQuantidade(TxtQuantidade.Text, out var quantidadeCompra) || quantidadeCompra <= 0)
            {
                ExibirErroValidacao("Informe a quantidade em estoque para calcular o custo unitário.");
                return false;
            }

            CalcularCustoUnitarioPeloTotal();
            FormattingHelper.TryParseMoedaOpcional(TxtCusto.Text, out custo);
        }

        if (custo is <= 0 && (_modoCustoTotal || !string.IsNullOrWhiteSpace(TxtCusto.Text)))
        {
            ExibirErroValidacao("Preço de custo, quando informado, deve ser maior que zero.");
            return false;
        }

        if (!FormattingHelper.TryParseMoeda(TxtPrecoVenda.Text, out decimal preco) || preco <= 0)
        {
            ExibirErroValidacao("Preço de venda deve ser maior que zero.");
            return false;
        }

        if (!FormattingHelper.TryParseQuantidade(TxtQuantidade.Text, out decimal quantidade) || quantidade < 0)
        {
            ExibirErroValidacao("Quantidade em estoque não pode ser negativa.");
            return false;
        }

        if (!FormattingHelper.TryParseQuantidade(TxtEstoqueMinimo.Text, out decimal estoqueMin) || estoqueMin < 0)
        {
            ExibirErroValidacao("Estoque mínimo não pode ser negativo.");
            return false;
        }

        return true;
    }

    private static int? ObterIdSelecionado(ComboBox combo)
    {
        return combo.SelectedValue switch
        {
            int id when id > 0 => id,
            long id when id > 0 => (int)id,
            _ when combo.SelectedItem is CategoriaDto c && c.Id > 0 => c.Id,
            _ when combo.SelectedItem is MarcaDto m && m.Id > 0 => m.Id,
            _ => null
        };
    }

    private void ExibirErroValidacao(string mensagem)
    {
        TxtErroValidacao.Text = mensagem;
        TxtErroValidacao.Visibility = Visibility.Visible;
        MessageBox.Show(mensagem, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void LimparErroValidacao()
    {
        TxtErroValidacao.Text = string.Empty;
        TxtErroValidacao.Visibility = Visibility.Collapsed;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
