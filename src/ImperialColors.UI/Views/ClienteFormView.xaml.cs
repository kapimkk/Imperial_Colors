using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.UI.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ImperialColors.UI.Views;

public partial class ClienteFormView : Window
{
    private readonly IClienteService _clienteService;
    private readonly IViaCepService _viaCepService;
    private int? _clienteId;
    private bool _suprimirMascaraCpf;
    private bool _suprimirMascaraCep;
    private bool _consultandoCep;
    private CancellationTokenSource? _cepCts;

    public ClienteFormView(IClienteService clienteService, IViaCepService viaCepService)
    {
        InitializeComponent();
        ModalWindowHelper.AplicarEstiloModerno(this);
        _clienteService = clienteService;
        _viaCepService = viaCepService;
        EnderecoFormHelper.AplicarMascaraUf(TxtEstado);
    }

    public void InicializarNovo()
    {
        TxtTitulo.Text = "Novo Cliente";
        _clienteId = null;
        TxtStatus.Text = string.Empty;
    }

    public void InicializarEdicao(ClienteDto cliente)
    {
        TxtTitulo.Text = "Editar Cliente";
        _clienteId = cliente.Id;
        TxtNome.Text = cliente.Nome;
        _suprimirMascaraCpf = true;
        TxtCpf.Text = DocumentoHelper.AplicarMascaraCpf(cliente.Cpf);
        _suprimirMascaraCpf = false;
        TxtTelefone.Text = cliente.Telefone ?? string.Empty;
        TxtWhatsApp.Text = cliente.WhatsApp ?? string.Empty;
        TxtEmail.Text = cliente.Email ?? string.Empty;
        _suprimirMascaraCep = true;
        TxtCep.Text = DocumentoHelper.AplicarMascaraCep(cliente.Cep);
        _suprimirMascaraCep = false;
        TxtLogradouro.Text = cliente.Logradouro ?? string.Empty;
        TxtNumero.Text = cliente.Numero ?? string.Empty;
        TxtComplemento.Text = cliente.Complemento ?? string.Empty;
        TxtBairro.Text = cliente.Bairro ?? string.Empty;
        TxtCidade.Text = cliente.Cidade ?? string.Empty;
        TxtEstado.Text = cliente.Estado ?? string.Empty;
        TxtObservacoes.Text = cliente.Observacoes ?? string.Empty;
        TxtStatus.Text = string.Empty;
    }

    private void TxtCpf_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suprimirMascaraCpf) return;

        _suprimirMascaraCpf = true;
        var cursor = TxtCpf.SelectionStart;
        var digitsAntes = DocumentoHelper.SomenteDigitos(TxtCpf.Text[..Math.Min(cursor, TxtCpf.Text.Length)]).Length;
        TxtCpf.Text = DocumentoHelper.AplicarMascaraCpf(TxtCpf.Text);
        TxtCpf.SelectionStart = Math.Min(TxtCpf.Text.Length, digitsAntes + (digitsAntes > 3 ? 1 : 0) + (digitsAntes > 6 ? 1 : 0) + (digitsAntes > 9 ? 1 : 0));
        _suprimirMascaraCpf = false;
    }

    private void TxtCep_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suprimirMascaraCep) return;

        _suprimirMascaraCep = true;
        TxtCep.Text = DocumentoHelper.AplicarMascaraCep(TxtCep.Text);
        TxtCep.SelectionStart = TxtCep.Text.Length;
        _suprimirMascaraCep = false;
    }

    private async void BtnBuscarCep_Click(object sender, RoutedEventArgs e)
        => await ConsultarCepAsync(exigirCepCompleto: true);

    private async Task ConsultarCepAsync(bool exigirCepCompleto = false)
    {
        if (_consultandoCep)
            return;

        if (!DocumentoHelper.CepCompleto(TxtCep.Text))
        {
            if (exigirCepCompleto)
            {
                TxtStatus.Text = "Informe um CEP válido com 8 dígitos.";
                TxtCep.Focus();
            }
            return;
        }

        _cepCts?.Cancel();
        _cepCts?.Dispose();
        _cepCts = new CancellationTokenSource();
        var token = _cepCts.Token;

        _consultandoCep = true;
        TxtStatus.Text = "Consultando CEP...";

        try
        {
            var endereco = await _viaCepService.ConsultarAsync(TxtCep.Text, token);
            if (token.IsCancellationRequested) return;

            if (endereco is null)
            {
                TxtStatus.Text = "CEP não encontrado.";
                return;
            }

            EnderecoFormHelper.PreencherEndereco(
                TxtLogradouro, TxtBairro, TxtCidade, TxtEstado,
                endereco.Logradouro, endereco.Bairro, endereco.Cidade, endereco.Uf);

            TxtStatus.Text = "Endereço preenchido automaticamente.";
            TxtNumero.Focus();
        }
        catch (OperationCanceledException) { }
        catch
        {
            TxtStatus.Text = "Não foi possível consultar o CEP.";
        }
        finally
        {
            _consultandoCep = false;
        }
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
                Cpf = DocumentoHelper.AplicarMascaraCpf(TxtCpf.Text),
                Telefone = TxtTelefone.Text.Trim(),
                WhatsApp = TxtWhatsApp.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                Cep = TxtCep.Text.Trim(),
                Logradouro = TxtLogradouro.Text.Trim(),
                Numero = TxtNumero.Text.Trim(),
                Complemento = TxtComplemento.Text.Trim(),
                Bairro = TxtBairro.Text.Trim(),
                Cidade = TxtCidade.Text.Trim(),
                Estado = TxtEstado.Text.Trim().ToUpperInvariant(),
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

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        => ModalWindowHelper.Fechar(this, false);

    private void BtnFechar_Click(object sender, RoutedEventArgs e)
        => ModalWindowHelper.Fechar(this, false);

    protected override void OnClosed(EventArgs e)
    {
        _cepCts?.Cancel();
        _cepCts?.Dispose();
        base.OnClosed(e);
    }
}
