using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class VendaViewModel : BaseViewModel
{
    public const int ItensPorPaginaPadrao = 50;

    private readonly IVendaService _vendaService;
    private readonly IServiceScopeFactory _scopeFactory;
    private CancellationTokenSource? _buscaCts;
    private readonly SemaphoreSlim _buscaSemaforo = new(1, 1);

    private ObservableCollection<VendaDto> _vendas = new();
    public ObservableCollection<VendaDto> Vendas { get => _vendas; set => SetProperty(ref _vendas, value); }

    private VendaDto? _vendaSelecionada;
    public VendaDto? VendaSelecionada
    {
        get => _vendaSelecionada;
        set { SetProperty(ref _vendaSelecionada, value); OnPropertyChanged(nameof(TemSelecao)); }
    }

    public bool TemSelecao => VendaSelecionada is not null;

    private DateTime _dataInicio = DateTime.Today.AddDays(-30);
    public DateTime DataInicio
    {
        get => _dataInicio;
        set => SetProperty(ref _dataInicio, value);
    }

    private DateTime _dataFim = DateTime.Today;
    public DateTime DataFim
    {
        get => _dataFim;
        set => SetProperty(ref _dataFim, value);
    }

    private string _termoBusca = string.Empty;
    public string TermoBusca
    {
        get => _termoBusca;
        set
        {
            if (!SetPropertyIfChanged(ref _termoBusca, value)) return;
            PaginaAtual = 1;
            _ = BuscarAsync();
        }
    }

    private int _paginaAtual = 1;
    public int PaginaAtual
    {
        get => _paginaAtual;
        set
        {
            if (!SetPropertyIfChanged(ref _paginaAtual, value)) return;
            OnPropertyChanged(nameof(InfoPaginacao));
            OnPropertyChanged(nameof(PodePaginaAnterior));
            OnPropertyChanged(nameof(PodePaginaProxima));
        }
    }

    private int _totalPaginas;
    public int TotalPaginas { get => _totalPaginas; set => SetProperty(ref _totalPaginas, value); }

    private int _totalItens;
    public int TotalItens { get => _totalItens; set => SetProperty(ref _totalItens, value); }

    public string InfoPaginacao => TotalPaginas <= 0 ? "Nenhuma venda" : $"Página {PaginaAtual} de {TotalPaginas}";
    public bool PodePaginaAnterior => PaginaAtual > 1 && !Carregando;
    public bool PodePaginaProxima => PaginaAtual < TotalPaginas && !Carregando;

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovaVendaCommand { get; }
    public AsyncRelayCommand VisualizarVendaCommand { get; }
    public AsyncRelayCommand CancelarVendaCommand { get; }
    public AsyncRelayCommand ImprimirCupomCommand { get; }
    public AsyncRelayCommand FiltrarCommand { get; }
    public AsyncRelayCommand PaginaAnteriorCommand { get; }
    public AsyncRelayCommand PaginaProximaCommand { get; }

    public VendaViewModel(IVendaService vendaService, IServiceScopeFactory scopeFactory)
    {
        _vendaService = vendaService;
        _scopeFactory = scopeFactory;

        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovaVendaCommand = new AsyncRelayCommand(AbrirPDV);
        VisualizarVendaCommand = new AsyncRelayCommand(VisualizarVenda, () => TemSelecao);
        CancelarVendaCommand = new AsyncRelayCommand(CancelarVenda, () => TemSelecao);
        ImprimirCupomCommand = new AsyncRelayCommand(ImprimirCupom, () => TemSelecao);
        FiltrarCommand = new AsyncRelayCommand(FiltrarAsync);
        PaginaAnteriorCommand = new AsyncRelayCommand(IrPaginaAnterior, () => PodePaginaAnterior);
        PaginaProximaCommand = new AsyncRelayCommand(IrPaginaProxima, () => PodePaginaProxima);
    }

    public async Task CarregarAsync()
    {
        PaginaAtual = Math.Max(1, PaginaAtual);
        await BuscarAsync();
    }

    private async Task FiltrarAsync()
    {
        PaginaAtual = 1;
        await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        _buscaCts?.Cancel();
        _buscaCts?.Dispose();
        _buscaCts = new CancellationTokenSource();
        var token = _buscaCts.Token;
        var semaforoAdquirido = false;

        try
        {
            await _buscaSemaforo.WaitAsync(token);
            semaforoAdquirido = true;
            if (token.IsCancellationRequested) return;

            UiDispatcher.ExecutarNaUi(() => Carregando = true);

            var resultado = await _vendaService.ObterPaginadoPorPeriodoAsync(
                DataInicio,
                DataFim.AddDays(1).AddSeconds(-1),
                PaginaAtual,
                ItensPorPaginaPadrao,
                string.IsNullOrWhiteSpace(TermoBusca) ? null : TermoBusca.Trim(),
                token).ConfigureAwait(false);

            if (token.IsCancellationRequested) return;

            UiDispatcher.ExecutarNaUi(() =>
            {
                Vendas = new ObservableCollection<VendaDto>(resultado.Itens);
                TotalItens = resultado.TotalItens;
                TotalPaginas = resultado.TotalPaginas;
                if (TotalPaginas > 0 && PaginaAtual > TotalPaginas)
                {
                    PaginaAtual = TotalPaginas;
                    _ = BuscarAsync();
                    return;
                }
                VendaSelecionada = null;
                OnPropertyChanged(nameof(InfoPaginacao));
                OnPropertyChanged(nameof(PodePaginaAnterior));
                OnPropertyChanged(nameof(PodePaginaProxima));
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { UiDispatcher.ExecutarNaUi(() => MostrarErro($"Erro ao carregar vendas: {ex.Message}")); }
        finally
        {
            if (semaforoAdquirido)
            {
                UiDispatcher.ExecutarNaUi(() => Carregando = false);
                _buscaSemaforo.Release();
            }
        }
    }

    private async Task IrPaginaAnterior() { if (PaginaAtual <= 1) return; PaginaAtual--; await BuscarAsync(); }
    private async Task IrPaginaProxima() { if (PaginaAtual >= TotalPaginas) return; PaginaAtual++; await BuscarAsync(); }

    private async Task AbrirPDV()
    {
        try
        {
            using var escopo = _scopeFactory.CreateScope();
            var pdv = escopo.ServiceProvider.GetRequiredService<Views.PDVView>();
            if (pdv.ShowDialog() == true)
                await CarregarAsync();
        }
        catch (Exception ex) { MostrarErro($"Erro ao abrir PDV: {ex.Message}"); }
    }

    private async Task VisualizarVenda()
    {
        if (VendaSelecionada is null) return;
        using var escopo = _scopeFactory.CreateScope();
        await WindowHelper.ExibirCupomAsync(escopo.ServiceProvider, VendaSelecionada);
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
        catch (Exception ex) { MostrarErro($"Erro ao cancelar: {ex.Message}"); }
    }

    private async Task ImprimirCupom()
    {
        if (VendaSelecionada is null) return;
        using var escopo = _scopeFactory.CreateScope();
        await WindowHelper.ExibirCupomAsync(escopo.ServiceProvider, VendaSelecionada);
    }

    private bool SetPropertyIfChanged<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value!;
        OnPropertyChanged(propertyName);
        return true;
    }
}
