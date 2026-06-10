using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using System.Windows;

namespace ImperialColors.UI.Views;

public partial class ClienteFormView : Window
{
    private readonly IClienteService _clienteService;
    private int? _clienteId;

    public ClienteFormView(IClienteService clienteService)
    {
        InitializeComponent();
        _clienteService = clienteService;
    }

    public void InicializarNovo() { TxtTitulo.Text = "Novo Cliente"; _clienteId = null; }

    public void InicializarEdicao(ClienteDto cliente)
    {
        TxtTitulo.Text = "Editar Cliente";
        _clienteId = cliente.Id;
        TxtNome.Text = cliente.Nome;
        TxtTelefone.Text = cliente.Telefone ?? "";
        TxtWhatsApp.Text = cliente.WhatsApp ?? "";
        TxtEmail.Text = cliente.Email ?? "";
        TxtCep.Text = cliente.Cep ?? "";
        TxtLogradouro.Text = cliente.Logradouro ?? "";
        TxtNumero.Text = cliente.Numero ?? "";
        TxtComplemento.Text = cliente.Complemento ?? "";
        TxtBairro.Text = cliente.Bairro ?? "";
        TxtCidade.Text = cliente.Cidade ?? "";
        TxtEstado.Text = cliente.Estado ?? "";
        TxtObservacoes.Text = cliente.Observacoes ?? "";
    }

    private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNome.Text))
        {
            MessageBox.Show("Nome é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dto = new ClienteDto
            {
                Nome = TxtNome.Text.Trim(),
                Telefone = TxtTelefone.Text.Trim(),
                WhatsApp = TxtWhatsApp.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                Cep = TxtCep.Text.Trim(),
                Logradouro = TxtLogradouro.Text.Trim(),
                Numero = TxtNumero.Text.Trim(),
                Complemento = TxtComplemento.Text.Trim(),
                Bairro = TxtBairro.Text.Trim(),
                Cidade = TxtCidade.Text.Trim(),
                Estado = TxtEstado.Text.Trim().ToUpper(),
                Observacoes = TxtObservacoes.Text.Trim()
            };

            if (_clienteId.HasValue)
                await _clienteService.AtualizarAsync(_clienteId.Value, dto);
            else
                await _clienteService.CriarAsync(dto);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
}
