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

    private int _alertasEstoqueCritico;
    public int AlertasEstoqueCritico { get => _alertasEstoqueCritico; set => SetProperty(ref _alertasEstoqueCritico, value); }

    private int _totalClientes;
    public int TotalClientes { get => _totalClientes; set => SetProperty(ref _totalClientes, value); }

    private List<ProdutoMaisVendidoDto> _topProdutosMes = new();
    public List<ProdutoMaisVendidoDto> TopProdutosMes { get => _topProdutosMes; set => SetProperty(ref _topProdutosMes, value); }

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
            AlertasEstoqueCritico = dados.AlertasEstoqueCritico;
            TotalClientes = dados.TotalClientes;
            TopProdutosMes = dados.TopProdutosMes;
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
