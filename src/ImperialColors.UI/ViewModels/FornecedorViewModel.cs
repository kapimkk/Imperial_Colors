using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using System.Collections.ObjectModel;

namespace ImperialColors.UI.ViewModels;

public class FornecedorViewModel : BaseViewModel
{
    private readonly IFornecedorService _fornecedorService;
    private readonly IServiceProvider _serviceProvider;

    private ObservableCollection<FornecedorDto> _fornecedores = new();
    public ObservableCollection<FornecedorDto> Fornecedores { get => _fornecedores; set => SetProperty(ref _fornecedores, value); }

    private FornecedorDto? _fornecedorSelecionado;
    public FornecedorDto? FornecedorSelecionado
    {
        get => _fornecedorSelecionado;
        set { SetProperty(ref _fornecedorSelecionado, value); OnPropertyChanged(nameof(TemSelecao)); }
    }

    public bool TemSelecao => FornecedorSelecionado is not null;

    private string _termoBusca = string.Empty;
    public string TermoBusca
    {
        get => _termoBusca;
        set { SetProperty(ref _termoBusca, value); _ = BuscarAsync(); }
    }

    public AsyncRelayCommand CarregarCommand { get; }
    public AsyncRelayCommand NovoFornecedorCommand { get; }
    public AsyncRelayCommand EditarFornecedorCommand { get; }
    public AsyncRelayCommand ExcluirFornecedorCommand { get; }

    public FornecedorViewModel(IFornecedorService fornecedorService, IServiceProvider serviceProvider)
    {
        _fornecedorService = fornecedorService;
        _serviceProvider = serviceProvider;
        CarregarCommand = new AsyncRelayCommand(CarregarAsync);
        NovoFornecedorCommand = new AsyncRelayCommand(AbrirNovo);
        EditarFornecedorCommand = new AsyncRelayCommand(AbrirEditar, () => TemSelecao);
        ExcluirFornecedorCommand = new AsyncRelayCommand(Excluir, () => TemSelecao);
    }

    public async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var lista = await _fornecedorService.ObterTodosAsync();
            Fornecedores = new ObservableCollection<FornecedorDto>(lista);
        }
        catch (Exception ex) { MostrarErro($"Erro: {ex.Message}"); }
        finally { Carregando = false; }
    }

    private async Task BuscarAsync()
    {
        try
        {
            var lista = await _fornecedorService.BuscarAsync(TermoBusca);
            Fornecedores = new ObservableCollection<FornecedorDto>(lista);
        }
        catch (Exception ex) { MostrarErro($"Erro: {ex.Message}"); }
    }

    private Task AbrirNovo()
    {
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.FornecedorFormView))!;
        if (janela is Views.FornecedorFormView form)
        {
            form.InicializarNovo();
            if (form.ShowDialog() == true) _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private Task AbrirEditar()
    {
        if (FornecedorSelecionado is null) return Task.CompletedTask;
        var janela = (System.Windows.Window)_serviceProvider.GetService(typeof(Views.FornecedorFormView))!;
        if (janela is Views.FornecedorFormView form)
        {
            form.InicializarEdicao(FornecedorSelecionado);
            if (form.ShowDialog() == true) _ = CarregarAsync();
        }
        return Task.CompletedTask;
    }

    private async Task Excluir()
    {
        if (FornecedorSelecionado is null) return;
        if (!ConfirmarAcao($"Deseja excluir o fornecedor '{FornecedorSelecionado.Nome}'?")) return;
        try
        {
            await _fornecedorService.RemoverAsync(FornecedorSelecionado.Id);
            MostrarSucesso("Fornecedor excluído!");
            await CarregarAsync();
        }
        catch (Exception ex) { MostrarErro($"Erro: {ex.Message}"); }
    }
}
