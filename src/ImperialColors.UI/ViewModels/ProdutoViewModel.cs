using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Enums;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class ProdutoViewModel : BaseViewModel
{
    private readonly IProdutoService _produtoService;
    private readonly IServiceProvider _serviceProvider;

    private ObservableCollection<ProdutoDto> _produtos = new();
    public ObservableCollection<ProdutoDto> Produtos { get => _produtos; set => SetProperty(ref _produtos, value); }

    private ProdutoDto? _produtoSelecionado;
    public ProdutoDto? ProdutoSelecionado
    {
        get => _produtoSelecionado;
        set
        {
            SetProperty(ref _produtoSelecionado, value);
            OnPropertyChanged(nameof(TemSelecao));
        }
    }

    public bool TemSelecao => ProdutoSelecionado is not null;

    private string _termoBusca = string.Empty;
    public string TermoBusca
    {
        get => _termoBusca;
        set
        {
            SetProperty(ref _termoBusca, value);
            _ = BuscarAsync();
        }
    }

    private int _totalProdutos;
    public int TotalProdutos { get => _totalProdutos; set => SetProperty(ref _totalProdutos, value); }

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovoProdutoCommand { get; }
    public AsyncRelayCommand EditarProdutoCommand { get; }
    public AsyncRelayCommand ExcluirProdutoCommand { get; }
    public AsyncRelayCommand MovimentacaoCommand { get; }

    public ProdutoViewModel(IProdutoService produtoService, IServiceProvider serviceProvider)
    {
        _produtoService = produtoService;
        _serviceProvider = serviceProvider;

        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovoProdutoCommand = new AsyncRelayCommand(AbrirNovoProduto);
        EditarProdutoCommand = new AsyncRelayCommand(AbrirEditarProduto, () => TemSelecao);
        ExcluirProdutoCommand = new AsyncRelayCommand(ExcluirProduto, () => TemSelecao);
        MovimentacaoCommand = new AsyncRelayCommand(AbrirMovimentacao, () => TemSelecao);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var produtos = await _produtoService.ObterTodosAsync();
            Produtos = new ObservableCollection<ProdutoDto>(produtos);
            TotalProdutos = Produtos.Count;
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao carregar produtos: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private async Task BuscarAsync()
    {
        try
        {
            var produtos = string.IsNullOrWhiteSpace(TermoBusca)
                ? await _produtoService.ObterTodosAsync()
                : await _produtoService.BuscarAsync(TermoBusca);
            Produtos = new ObservableCollection<ProdutoDto>(produtos);
            TotalProdutos = Produtos.Count;
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro na busca: {ex.Message}");
        }
    }

    private Task AbrirNovoProduto()
    {
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.ProdutoFormView))!;
        if (janela is Views.ProdutoFormView form)
        {
            form.InicializarNovo();
            if (form.ShowDialog() == true)
                _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private Task AbrirEditarProduto()
    {
        if (ProdutoSelecionado is null) return Task.CompletedTask;
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.ProdutoFormView))!;
        if (janela is Views.ProdutoFormView form)
        {
            form.InicializarEdicao(ProdutoSelecionado);
            if (form.ShowDialog() == true)
                _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private async Task ExcluirProduto()
    {
        if (ProdutoSelecionado is null) return;
        if (!ConfirmarAcao($"Deseja excluir o produto '{ProdutoSelecionado.Nome}'?")) return;

        try
        {
            await _produtoService.RemoverAsync(ProdutoSelecionado.Id);
            MostrarSucesso("Produto excluído com sucesso!");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao excluir: {ex.Message}");
        }
    }

    private Task AbrirMovimentacao()
    {
        if (ProdutoSelecionado is null) return Task.CompletedTask;
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.MovimentacaoEstoqueView))!;
        if (janela is Views.MovimentacaoEstoqueView form)
        {
            form.InicializarProduto(ProdutoSelecionado);
            if (form.ShowDialog() == true)
                _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }
}
