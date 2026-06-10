using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using System.Windows;

namespace ImperialColors.UI.Views;

public partial class FornecedorFormView : Window
{
    private readonly IFornecedorService _fornecedorService;
    private int? _fornecedorId;

    public FornecedorFormView(IFornecedorService fornecedorService)
    {
        InitializeComponent();
        _fornecedorService = fornecedorService;
    }

    public void InicializarNovo() { TxtTitulo.Text = "Novo Fornecedor"; _fornecedorId = null; }

    public void InicializarEdicao(FornecedorDto fornecedor)
    {
        TxtTitulo.Text = "Editar Fornecedor";
        _fornecedorId = fornecedor.Id;
        TxtNome.Text = fornecedor.Nome;
        TxtTelefone.Text = fornecedor.Telefone ?? "";
        TxtWhatsApp.Text = fornecedor.WhatsApp ?? "";
        TxtEmail.Text = fornecedor.Email ?? "";
        TxtCep.Text = fornecedor.Cep ?? "";
        TxtLogradouro.Text = fornecedor.Logradouro ?? "";
        TxtNumero.Text = fornecedor.Numero ?? "";
        TxtComplemento.Text = fornecedor.Complemento ?? "";
        TxtBairro.Text = fornecedor.Bairro ?? "";
        TxtCidade.Text = fornecedor.Cidade ?? "";
        TxtEstado.Text = fornecedor.Estado ?? "";
        TxtObservacoes.Text = fornecedor.Observacoes ?? "";
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
            var dto = new FornecedorDto
            {
                Nome = TxtNome.Text.Trim(), Telefone = TxtTelefone.Text.Trim(), WhatsApp = TxtWhatsApp.Text.Trim(),
                Email = TxtEmail.Text.Trim(), Cep = TxtCep.Text.Trim(), Logradouro = TxtLogradouro.Text.Trim(),
                Numero = TxtNumero.Text.Trim(), Complemento = TxtComplemento.Text.Trim(), Bairro = TxtBairro.Text.Trim(),
                Cidade = TxtCidade.Text.Trim(), Estado = TxtEstado.Text.Trim().ToUpper(), Observacoes = TxtObservacoes.Text.Trim()
            };
            if (_fornecedorId.HasValue) await _fornecedorService.AtualizarAsync(_fornecedorId.Value, dto);
            else await _fornecedorService.CriarAsync(dto);
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
