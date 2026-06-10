using ImperialColors.UI.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImperialColors.UI.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _carregando;
    public bool Carregando
    {
        get => _carregando;
        set => SetProperty(ref _carregando, value);
    }

    private string _mensagemErro = string.Empty;
    public string MensagemErro
    {
        get => _mensagemErro;
        set => SetProperty(ref _mensagemErro, value);
    }

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected void MostrarErro(string mensagem)
    {
        UiDispatcher.ExecutarNaUi(() =>
        {
            MensagemErro = mensagem;
            MessageBox.Show(mensagem, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    protected void MostrarSucesso(string mensagem)
        => UiDispatcher.ExecutarNaUi(() =>
            MessageBox.Show(mensagem, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information));

    protected bool ConfirmarAcao(string mensagem)
    {
        var confirmou = false;
        UiDispatcher.ExecutarNaUi(() =>
            confirmou = MessageBox.Show(mensagem, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
        return confirmou;
    }
}
