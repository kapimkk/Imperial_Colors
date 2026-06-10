using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ImperialColors.UI.Views;

public partial class ProdutoFormView : Window
{
    private readonly IProdutoService _produtoService;
    private readonly IRepository<Categoria> _categoriaRepository;
    private readonly IRepository<Marca> _marcaRepository;
    private int? _produtoId;

    public ProdutoFormView(
        IProdutoService produtoService,
        IRepository<Categoria> categoriaRepository,
        IRepository<Marca> marcaRepository)
    {
        InitializeComponent();
        _produtoService = produtoService;
        _categoriaRepository = categoriaRepository;
        _marcaRepository = marcaRepository;
        _ = CarregarComboBoxesAsync();
    }

    private async Task CarregarComboBoxesAsync()
    {
        var categorias = await _categoriaRepository.ObterTodosAsync();
        var marcas = await _marcaRepository.ObterTodosAsync();

        var categoriasLista = new List<Categoria> { new() { Id = 0, Nome = "Sem categoria" } };
        categoriasLista.AddRange(categorias.OrderBy(c => c.Nome));
        CmbCategoria.ItemsSource = categoriasLista;
        CmbCategoria.SelectedIndex = 0;

        var marcasLista = new List<Marca> { new() { Id = 0, Nome = "Sem marca" } };
        marcasLista.AddRange(marcas.OrderBy(m => m.Nome));
        CmbMarca.ItemsSource = marcasLista;
        CmbMarca.SelectedIndex = 0;
    }

    public void InicializarNovo()
    {
        TxtTitulo.Text = "Novo Produto";
        _produtoId = null;
        _ = GerarCodigoAsync();
    }

    public void InicializarEdicao(ProdutoDto produto)
    {
        TxtTitulo.Text = "Editar Produto";
        _produtoId = produto.Id;
        TxtCodigoInterno.Text = produto.CodigoInterno;
        TxtCodigoBarras.Text = produto.CodigoBarras ?? "";
        TxtNome.Text = produto.Nome;
        TxtQuantidade.Text = produto.QuantidadeEstoque.ToString("G", new CultureInfo("pt-BR"));
        TxtEstoqueMinimo.Text = produto.EstoqueMinimo.ToString("G", new CultureInfo("pt-BR"));
        TxtCusto.Text = produto.Custo.ToString("F2", new CultureInfo("pt-BR"));
        TxtPrecoVenda.Text = produto.PrecoVenda.ToString("F2", new CultureInfo("pt-BR"));
        TxtObservacoes.Text = produto.Observacoes ?? "";

        foreach (System.Windows.Controls.ComboBoxItem item in CmbUnidade.Items)
            if (item.Content?.ToString() == produto.Unidade) { CmbUnidade.SelectedItem = item; break; }

        Dispatcher.InvokeAsync(() =>
        {
            if (produto.CategoriaId.HasValue && produto.CategoriaId > 0)
                CmbCategoria.SelectedValue = produto.CategoriaId;
            if (produto.MarcaId.HasValue && produto.MarcaId > 0)
                CmbMarca.SelectedValue = produto.MarcaId;
        });
    }

    private async Task GerarCodigoAsync()
    {
        var codigo = await _produtoService.GerarProximoCodigoInternoAsync();
        TxtCodigoInterno.Text = codigo;
    }

    private void BtnGerarCodigo_Click(object sender, RoutedEventArgs e) => _ = GerarCodigoAsync();

    private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text))
            {
                MessageBox.Show("Nome do produto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPrecoVenda.Text.Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco) || preco <= 0)
            {
                MessageBox.Show("Preço de venda inválido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(TxtCusto.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal custo);
            decimal.TryParse(TxtQuantidade.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal quantidade);
            decimal.TryParse(TxtEstoqueMinimo.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal estoqueMin);

            var categoriaId = CmbCategoria.SelectedValue is int cid && cid > 0 ? (int?)cid : null;
            var marcaId = CmbMarca.SelectedValue is int mid && mid > 0 ? (int?)mid : null;
            var unidade = (CmbUnidade.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "UN";

            BtnSalvar.IsEnabled = false;
            BtnSalvar.Content = "Salvando...";

            if (_produtoId.HasValue)
            {
                var dto = new AtualizarProdutoDto
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
                };
                await _produtoService.AtualizarAsync(_produtoId.Value, dto);
            }
            else
            {
                var dto = new CriarProdutoDto
                {
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
                };
                await _produtoService.CriarAsync(dto);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            BtnSalvar.IsEnabled = true;
            BtnSalvar.Content = "Salvar Produto";
            MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
