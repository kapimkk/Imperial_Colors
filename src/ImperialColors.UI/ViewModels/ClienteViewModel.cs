using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class ClienteViewModel : BaseViewModel
{
    private readonly IClienteService _clienteService;
    private readonly IServiceProvider _serviceProvider;

    private ObservableCollection<ClienteDto> _clientes = new();
    public ObservableCollection<ClienteDto> Clientes { get => _clientes; set => SetProperty(ref _clientes, value); }

    private ClienteDto? _clienteSelecionado;
    public ClienteDto? ClienteSelecionado
    {
        get => _clienteSelecionado;
        set { SetProperty(ref _clienteSelecionado, value); OnPropertyChanged(nameof(TemSelecao)); }
    }

    public bool TemSelecao => ClienteSelecionado is not null;

    private string _termoBusca = string.Empty;
    public string TermoBusca
    {
        get => _termoBusca;
        set { SetProperty(ref _termoBusca, value); _ = BuscarAsync(); }
    }

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovoClienteCommand { get; }
    public AsyncRelayCommand EditarClienteCommand { get; }
    public AsyncRelayCommand ExcluirClienteCommand { get; }

    public ClienteViewModel(IClienteService clienteService, IServiceProvider serviceProvider)
    {
        _clienteService = clienteService;
        _serviceProvider = serviceProvider;
        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovoClienteCommand = new AsyncRelayCommand(AbrirNovoCliente);
        EditarClienteCommand = new AsyncRelayCommand(AbrirEditarCliente, () => TemSelecao);
        ExcluirClienteCommand = new AsyncRelayCommand(ExcluirCliente, () => TemSelecao);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var clientes = await _clienteService.ObterTodosAsync();
            Clientes = new ObservableCollection<ClienteDto>(clientes);
        }
        catch (Exception ex) { MostrarErro($"Erro: {ex.Message}"); }
        finally { Carregando = false; }
    }

    private async Task BuscarAsync()
    {
        try
        {
            var clientes = await _clienteService.BuscarAsync(TermoBusca);
            Clientes = new ObservableCollection<ClienteDto>(clientes);
        }
        catch (Exception ex) { MostrarErro($"Erro na busca: {ex.Message}"); }
    }

    private Task AbrirNovoCliente()
    {
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.ClienteFormView))!;
        if (janela is Views.ClienteFormView form)
        {
            form.InicializarNovo();
            if (form.ShowDialog() == true) _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private Task AbrirEditarCliente()
    {
        if (ClienteSelecionado is null) return Task.CompletedTask;
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.ClienteFormView))!;
        if (janela is Views.ClienteFormView form)
        {
            form.InicializarEdicao(ClienteSelecionado);
            if (form.ShowDialog() == true) _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private async Task ExcluirCliente()
    {
        if (ClienteSelecionado is null) return;
        if (!ConfirmarAcao($"Deseja excluir o cliente '{ClienteSelecionado.Nome}'?")) return;
        try
        {
            await _clienteService.RemoverAsync(ClienteSelecionado.Id);
            MostrarSucesso("Cliente excluído!");
            await CarregarAsync();
        }
        catch (Exception ex) { MostrarErro($"Erro: {ex.Message}"); }
    }
}
