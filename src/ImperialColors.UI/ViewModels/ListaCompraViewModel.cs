using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class ListaCompraViewModel : BaseViewModel
{
    private readonly IListaCompraService _listaCompraService;
    private readonly IServiceScopeFactory _scopeFactory;

    private ObservableCollection<ListaCompraDto> _listas = new();
    public ObservableCollection<ListaCompraDto> Listas { get => _listas; set => SetProperty(ref _listas, value); }

    private ListaCompraDto? _listaSelecionada;
    public ListaCompraDto? ListaSelecionada
    {
        get => _listaSelecionada;
        set
        {
            SetProperty(ref _listaSelecionada, value);
            OnPropertyChanged(nameof(TemSelecao));
            OnPropertyChanged(nameof(PodeFinalizar));
            OnPropertyChanged(nameof(PodeReabrir));
        }
    }

    public bool TemSelecao => ListaSelecionada is not null;
    public bool PodeFinalizar => TemSelecao && ListaSelecionada is { Finalizada: false };
    public bool PodeReabrir => TemSelecao && ListaSelecionada is { Finalizada: true };

    private string _termoBusca = string.Empty;
    public string TermoBusca
    {
        get => _termoBusca;
        set
        {
            if (!SetPropertyIfChanged(ref _termoBusca, value)) return;
            _ = CarregarAsync();
        }
    }

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovaListaCommand { get; }
    public AsyncRelayCommand EditarListaCommand { get; }
    public AsyncRelayCommand ExcluirListaCommand { get; }
    public AsyncRelayCommand FinalizarListaCommand { get; }
    public AsyncRelayCommand ReabrirListaCommand { get; }

    public ListaCompraViewModel(IListaCompraService listaCompraService, IServiceScopeFactory scopeFactory)
    {
        _listaCompraService = listaCompraService;
        _scopeFactory = scopeFactory;

        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovaListaCommand = new AsyncRelayCommand(AbrirNova);
        EditarListaCommand = new AsyncRelayCommand(AbrirEditar, () => TemSelecao && !Carregando);
        ExcluirListaCommand = new AsyncRelayCommand(Excluir, () => TemSelecao && !Carregando);
        FinalizarListaCommand = new AsyncRelayCommand(Finalizar, () => PodeFinalizar && !Carregando);
        ReabrirListaCommand = new AsyncRelayCommand(Reabrir, () => PodeReabrir && !Carregando);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var listas = await _listaCompraService.ObterTodosAsync(
                string.IsNullOrWhiteSpace(TermoBusca) ? null : TermoBusca.Trim());
            Listas = new ObservableCollection<ListaCompraDto>(listas);
            ListaSelecionada = null;
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao carregar listas: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private Task AbrirNova()
    {
        try
        {
            using var escopo = _scopeFactory.CreateScope();
            var form = escopo.ServiceProvider.GetRequiredService<Views.ListaCompraFormView>();
            form.InicializarNova();
            if (ModalWindowHelper.ExibirDialogo(form) == true)
                _ = CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao abrir lista: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private Task AbrirEditar()
    {
        if (ListaSelecionada is null) return Task.CompletedTask;

        try
        {
            using var escopo = _scopeFactory.CreateScope();
            var form = escopo.ServiceProvider.GetRequiredService<Views.ListaCompraFormView>();
            form.InicializarEdicao(ListaSelecionada);
            if (ModalWindowHelper.ExibirDialogo(form) == true)
                _ = CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao abrir edição: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private async Task Excluir()
    {
        if (ListaSelecionada is null) return;
        if (!ConfirmarAcao($"Deseja excluir a lista '{ListaSelecionada.Nome}'?")) return;

        try
        {
            await _listaCompraService.RemoverAsync(ListaSelecionada.Id);
            MostrarSucesso("Lista excluída!");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao excluir: {ex.Message}");
        }
    }

    private async Task Finalizar()
    {
        if (ListaSelecionada is null) return;
        try
        {
            await _listaCompraService.AlterarFinalizadaAsync(ListaSelecionada.Id, true);
            MostrarSucesso("Lista finalizada!");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao finalizar: {ex.Message}");
        }
    }

    private async Task Reabrir()
    {
        if (ListaSelecionada is null) return;
        try
        {
            await _listaCompraService.AlterarFinalizadaAsync(ListaSelecionada.Id, false);
            MostrarSucesso("Lista reaberta!");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao reabrir: {ex.Message}");
        }
    }

    private bool SetPropertyIfChanged<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value!;
        OnPropertyChanged(propertyName);
        return true;
    }
}
