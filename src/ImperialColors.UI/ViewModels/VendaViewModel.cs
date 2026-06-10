using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class VendaViewModel : BaseViewModel
{
    private readonly IVendaService _vendaService;
    private readonly IServiceProvider _serviceProvider;

    private ObservableCollection<VendaDto> _vendas = new();
    public ObservableCollection<VendaDto> Vendas { get => _vendas; set => SetProperty(ref _vendas, value); }

    private VendaDto? _vendaSelecionada;
    public VendaDto? VendaSelecionada
    {
        get => _vendaSelecionada;
        set
        {
            SetProperty(ref _vendaSelecionada, value);
            OnPropertyChanged(nameof(TemSelecao));
        }
    }

    public bool TemSelecao => VendaSelecionada is not null;

    private DateTime _dataInicio = DateTime.Today.AddDays(-30);
    public DateTime DataInicio { get => _dataInicio; set => SetProperty(ref _dataInicio, value); }

    private DateTime _dataFim = DateTime.Today;
    public DateTime DataFim { get => _dataFim; set => SetProperty(ref _dataFim, value); }

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovaVendaCommand { get; }
    public AsyncRelayCommand VisualizarVendaCommand { get; }
    public AsyncRelayCommand CancelarVendaCommand { get; }
    public AsyncRelayCommand ImprimirCupomCommand { get; }
    public AsyncRelayCommand FiltrarCommand { get; }

    public VendaViewModel(IVendaService vendaService, IServiceProvider serviceProvider)
    {
        _vendaService = vendaService;
        _serviceProvider = serviceProvider;

        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovaVendaCommand = new AsyncRelayCommand(AbrirPDV);
        VisualizarVendaCommand = new AsyncRelayCommand(VisualizarVenda, () => TemSelecao);
        CancelarVendaCommand = new AsyncRelayCommand(CancelarVenda, () => TemSelecao);
        ImprimirCupomCommand = new AsyncRelayCommand(ImprimirCupom, () => TemSelecao);
        FiltrarCommand = new AsyncRelayCommand(FiltrarAsync);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var vendas = await _vendaService.ObterPorPeriodoAsync(DataInicio, DataFim.AddDays(1).AddSeconds(-1));
            Vendas = new ObservableCollection<VendaDto>(vendas);
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao carregar vendas: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private async Task FiltrarAsync() => await CarregarAsync();

    private Task AbrirPDV()
    {
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.PDVView))!;
        if (janela is Views.PDVView pdv)
        {
            if (pdv.ShowDialog() == true)
                _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private Task VisualizarVenda()
    {
        if (VendaSelecionada is null) return Task.CompletedTask;
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.CupomView))!;
        if (janela is Views.CupomView cupom)
        {
            cupom.InicializarVenda(VendaSelecionada);
            cupom.ShowDialog();
        }
        return Task.CompletedTask;
    }

    private async Task CancelarVenda()
    {
        if (VendaSelecionada is null) return;
        if (!ConfirmarAcao($"Deseja cancelar a venda #{VendaSelecionada.NumeroVenda}?")) return;

        try
        {
            await _vendaService.CancelarAsync(VendaSelecionada.Id);
            MostrarSucesso("Venda cancelada com sucesso!");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao cancelar: {ex.Message}");
        }
    }

    private Task ImprimirCupom()
    {
        if (VendaSelecionada is null) return Task.CompletedTask;
        VisualizarVenda().Wait();
        return Task.CompletedTask;
    }
}
