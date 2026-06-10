using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;

namespace ImperialColors.UI.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;

    private decimal _totalVendasHoje;
    public decimal TotalVendasHoje { get => _totalVendasHoje; set => SetProperty(ref _totalVendasHoje, value); }

    private decimal _totalVendasMes;
    public decimal TotalVendasMes { get => _totalVendasMes; set => SetProperty(ref _totalVendasMes, value); }

    private int _qtdVendasHoje;
    public int QtdVendasHoje { get => _qtdVendasHoje; set => SetProperty(ref _qtdVendasHoje, value); }

    private int _produtosEstoqueBaixo;
    public int ProdutosEstoqueBaixo { get => _produtosEstoqueBaixo; set => SetProperty(ref _produtosEstoqueBaixo, value); }

    private int _produtosSemEstoque;
    public int ProdutosSemEstoque { get => _produtosSemEstoque; set => SetProperty(ref _produtosSemEstoque, value); }

    private int _totalClientes;
    public int TotalClientes { get => _totalClientes; set => SetProperty(ref _totalClientes, value); }

    private int _totalProdutos;
    public int TotalProdutos { get => _totalProdutos; set => SetProperty(ref _totalProdutos, value); }

    private List<ProdutoBaixoEstoqueDto> _produtosBaixoEstoque = new();
    public List<ProdutoBaixoEstoqueDto> ProdutosBaixoEstoque { get => _produtosBaixoEstoque; set => SetProperty(ref _produtosBaixoEstoque, value); }

    public string DataHoje => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));

    public AsyncRelayCommand CarregarCommand { get; }

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        CarregarCommand = new AsyncRelayCommand(CarregarDados);
    }

    public async Task CarregarDados()
    {
        try
        {
            Carregando = true;
            var dados = await _dashboardService.ObterDadosDashboardAsync();
            TotalVendasHoje = dados.TotalVendasHoje;
            TotalVendasMes = dados.TotalVendasMes;
            QtdVendasHoje = dados.QuantidadeVendasHoje;
            ProdutosEstoqueBaixo = dados.ProdutosEstoqueBaixo;
            ProdutosSemEstoque = dados.ProdutosSemEstoque;
            TotalClientes = dados.TotalClientes;
            TotalProdutos = dados.TotalProdutos;
            ProdutosBaixoEstoque = dados.ProdutosBaixoEstoque;
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao carregar dashboard: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }
}
