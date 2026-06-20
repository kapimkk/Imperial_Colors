using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Exceptions;
using ImperialColors.UI.Helpers;
using ImperialColors.UI.Services;
using ImperialColors.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class VendaExternaViewModel : BaseViewModel
{
    private readonly IVendaExternaService _vendaExternaService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISessaoService _sessaoService;

    private ObservableCollection<VendaExternaDto> _vendas = new();
    public ObservableCollection<VendaExternaDto> Vendas { get => _vendas; set => SetProperty(ref _vendas, value); }

    private VendaExternaDto? _vendaSelecionada;
    public VendaExternaDto? VendaSelecionada
    {
        get => _vendaSelecionada;
        set
        {
            SetProperty(ref _vendaSelecionada, value);
            OnPropertyChanged(nameof(TemSelecao));
            NotifyCanExecuteChanged();
        }
    }

    public bool TemSelecao => VendaSelecionada is not null;

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand RegistrarVendaCommand { get; }

    public VendaExternaViewModel(
        IVendaExternaService vendaExternaService,
        IServiceScopeFactory scopeFactory,
        ISessaoService sessaoService)
    {
        _vendaExternaService = vendaExternaService;
        _scopeFactory = scopeFactory;
        _sessaoService = sessaoService;

        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        RegistrarVendaCommand = new AsyncRelayCommand(AbrirRegistro);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var vendas = await _vendaExternaService.ObterTodosAsync();
            Vendas = new ObservableCollection<VendaExternaDto>(vendas);
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao carregar vendas externas:\n\n{ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private async Task AbrirRegistro()
    {
        using var scope = _scopeFactory.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<VendaExternaFormView>();
        form.Owner = System.Windows.Application.Current.MainWindow;
        form.InicializarNova(_sessaoService.UsuarioAtual?.NomeCompleto);

        if (form.ShowDialog() == true)
            await CarregarAsync();
    }
}
