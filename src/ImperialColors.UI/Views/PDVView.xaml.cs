using ImperialColors.Application.DTOs;
using ImperialColors.Application.Helpers;
using ImperialColors.Application.Interfaces;
using ImperialColors.UI.Helpers;
using ImperialColors.UI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImperialColors.UI.Views;

public partial class PDVView : Window, INotifyPropertyChanged
{
    private readonly IProdutoService _produtoService;
    private readonly IVendaService _vendaService;
    private readonly IClienteService _clienteService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISessaoService _sessaoService;

    public event PropertyChangedEventHandler? PropertyChanged;

    private TipoDescontoVenda _tipoDesconto = TipoDescontoVenda.Valor;
    private decimal _descontoEmReais;
    private ObservableCollection<ItemVendaDto> _itensVenda = new();
    public ObservableCollection<ItemVendaDto> ItensVenda
    {
        get => _itensVenda;
        set { _itensVenda = value; NotifyPropertyChanged(); AtualizarTotais(); }
    }

    private decimal _subtotal;
    public decimal Subtotal { get => _subtotal; set { _subtotal = value; NotifyPropertyChanged(); } }

    private decimal _total;
    public decimal Total { get => _total; set { _total = value; NotifyPropertyChanged(); } }

    private int _totalItens;
    public int TotalItens { get => _totalItens; set { _totalItens = value; NotifyPropertyChanged(); } }

    private List<ProdutoDto> _produtosEncontrados = new();
    private string _numeroVenda = string.Empty;
    private CancellationTokenSource? _leitorBarrasCts;
    private bool _suprimirBuscaTextChanged;

    private const int AtrasoLeitorBarrasMs = 150;

    public PDVView(IProdutoService produtoService, IVendaService vendaService,
                   IClienteService clienteService, IServiceProvider serviceProvider,
                   ISessaoService sessaoService)
    {
        InitializeComponent();
        DataContext = this;
        _produtoService = produtoService;
        _vendaService = vendaService;
        _clienteService = clienteService;
        _serviceProvider = serviceProvider;
        _sessaoService = sessaoService;
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        var clientes = await _clienteService.ObterTodosAsync();
        var clientesLista = new List<ClienteDto> { new() { Id = 0, Nome = "Consumidor final" } };
        clientesLista.AddRange(clientes.OrderBy(c => c.Nome));
        CmbCliente.ItemsSource = clientesLista;
        CmbCliente.SelectedIndex = 0;
    }

    private async void TxtBuscaProduto_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suprimirBuscaTextChanged)
            return;

        var termo = TxtBuscaProduto.Text.Trim();
        if (termo.Length < 2)
        {
            PopupResultados.IsOpen = false;
            return;
        }

        if (PareceCodigoBarras(termo))
        {
            PopupResultados.IsOpen = false;
            await TentarAutoAdicionarPorCodigoBarrasAsync(termo);
            return;
        }

        _produtosEncontrados = (await _produtoService.BuscarAsync(termo)).ToList();
        if (_produtosEncontrados.Any())
        {
            LstResultados.ItemsSource = _produtosEncontrados;
            PopupResultados.IsOpen = true;
        }
        else
        {
            PopupResultados.IsOpen = false;
        }
    }

    private async void TxtBuscaProduto_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await ProcessarEntradaBuscaAsync();
        }

        if (e.Key == Key.Escape)
            PopupResultados.IsOpen = false;
    }

    private async void BtnAdicionarProduto_Click(object sender, RoutedEventArgs e)
        => await ProcessarEntradaBuscaAsync();

    /// <summary>
    /// Leitores USB digitam o código rapidamente e enviam Enter; busca exata evita popup e confirmação.
    /// </summary>
    private async Task ProcessarEntradaBuscaAsync()
    {
        CancelarLeituraBarrasPendente();

        var termo = TxtBuscaProduto.Text.Trim();
        if (string.IsNullOrWhiteSpace(termo))
            return;

        var porBarras = await _produtoService.ObterPorCodigoBarrasAsync(termo);
        if (porBarras is not null)
        {
            AdicionarItemVenda(porBarras);
            return;
        }

        var porCodigoInterno = await _produtoService.ObterPorCodigoInternoAsync(termo);
        if (porCodigoInterno is not null)
        {
            AdicionarItemVenda(porCodigoInterno);
            return;
        }

        _produtosEncontrados = (await _produtoService.BuscarAsync(termo)).ToList();

        if (_produtosEncontrados.Count == 1)
        {
            AdicionarItemVenda(_produtosEncontrados[0]);
            return;
        }

        if (_produtosEncontrados.Count > 1)
        {
            if (LstResultados.SelectedItem is ProdutoDto selecionado)
            {
                AdicionarItemVenda(selecionado);
                return;
            }

            LstResultados.ItemsSource = _produtosEncontrados;
            PopupResultados.IsOpen = true;
            return;
        }

        PopupResultados.IsOpen = false;
        MessageBox.Show(
            $"Nenhum produto encontrado para \"{termo}\".",
            "Produto não encontrado",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private async Task TentarAutoAdicionarPorCodigoBarrasAsync(string termo)
    {
        CancelarLeituraBarrasPendente();
        _leitorBarrasCts = new CancellationTokenSource();
        var token = _leitorBarrasCts.Token;

        try
        {
            await Task.Delay(AtrasoLeitorBarrasMs, token);

            if (token.IsCancellationRequested)
                return;

            var termoAtual = TxtBuscaProduto.Text.Trim();
            if (!string.Equals(termoAtual, termo, StringComparison.Ordinal))
                return;

            var produto = await _produtoService.ObterPorCodigoBarrasAsync(termoAtual);
            if (produto is not null)
                AdicionarItemVenda(produto);
        }
        catch (OperationCanceledException)
        {
            // Nova digitação ou Enter cancelou a leitura pendente.
        }
    }

    private void CancelarLeituraBarrasPendente()
    {
        _leitorBarrasCts?.Cancel();
        _leitorBarrasCts?.Dispose();
        _leitorBarrasCts = null;
    }

    private static bool PareceCodigoBarras(string termo)
        => termo.Length >= 4 && termo.All(char.IsDigit);

    private void LstResultados_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    private void LstResultados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LstResultados.SelectedItem is ProdutoDto produto)
        {
            AdicionarItemVenda(produto);
            PopupResultados.IsOpen = false;
        }
    }

    private void AdicionarItemVenda(ProdutoDto produto)
    {
        if (produto.SemEstoque)
        {
            MessageBox.Show($"Produto '{produto.Nome}' sem estoque!", "Sem Estoque", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var itemExistente = ItensVenda.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (itemExistente is not null)
        {
            if (itemExistente.Quantidade + 1 > produto.QuantidadeEstoque)
            {
                MessageBox.Show($"Estoque insuficiente! Disponível: {produto.QuantidadeEstoque} {produto.Unidade}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            itemExistente.Quantidade += 1;
            itemExistente.Subtotal = itemExistente.Quantidade * itemExistente.PrecoUnitario - itemExistente.Desconto;
            DgItens.Items.Refresh();
        }
        else
        {
            ItensVenda.Add(new ItemVendaDto
            {
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                CodigoInterno = produto.CodigoInterno,
                Quantidade = 1,
                PrecoUnitario = produto.PrecoVenda,
                Desconto = 0,
                Subtotal = produto.PrecoVenda,
                Unidade = produto.Unidade
            });
        }

        CancelarLeituraBarrasPendente();
        PopupResultados.IsOpen = false;
        _suprimirBuscaTextChanged = true;
        TxtBuscaProduto.Clear();
        _suprimirBuscaTextChanged = false;
        TxtBuscaProduto.Focus();
        AtualizarTotais();
    }

    private void BtnRemoverItem_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ItemVendaDto item)
        {
            ItensVenda.Remove(item);
            AtualizarTotais();
        }
    }

    private void BtnAumentarQtd_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ItemVendaDto item)
        {
            item.Quantidade += 1;
            item.Subtotal = item.Quantidade * item.PrecoUnitario - item.Desconto;
            DgItens.Items.Refresh();
            AtualizarTotais();
        }
    }

    private void BtnDiminuirQtd_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ItemVendaDto item)
        {
            if (item.Quantidade <= 1)
            {
                ItensVenda.Remove(item);
            }
            else
            {
                item.Quantidade -= 1;
                item.Subtotal = item.Quantidade * item.PrecoUnitario - item.Desconto;
                DgItens.Items.Refresh();
            }
            AtualizarTotais();
        }
    }

    private void TxtQtdItem_LostFocus(object sender, RoutedEventArgs e)
    {
        if ((sender as TextBox)?.Tag is ItemVendaDto item)
        {
            if (decimal.TryParse(((TextBox)sender).Text, out decimal qtd) && qtd > 0)
            {
                item.Quantidade = qtd;
                item.Subtotal = item.Quantidade * item.PrecoUnitario - item.Desconto;
                DgItens.Items.Refresh();
                AtualizarTotais();
            }
        }
    }

    private void BtnLimpar_Click(object sender, RoutedEventArgs e)
    {
        if (ItensVenda.Any() &&
            MessageBox.Show("Limpar todos os itens?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            ItensVenda.Clear();
            AtualizarTotais();
        }
    }

    private void TxtDesconto_TextChanged(object sender, TextChangedEventArgs e) => AtualizarTotais();

    private void CmbTipoDesconto_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTipoDesconto?.SelectedItem is not ComboBoxItem item) return;
        _tipoDesconto = item.Content?.ToString() == "%" ? TipoDescontoVenda.Percentual : TipoDescontoVenda.Valor;
        AtualizarTotais();
    }

    private void AtualizarTotais()
    {
        Subtotal = ItensVenda.Sum(i => i.Subtotal);

        var valorInformado = ObterValorDescontoInformado();
        if (_tipoDesconto == TipoDescontoVenda.Percentual && !DescontoHelper.PercentualValido(valorInformado))
            valorInformado = Math.Clamp(valorInformado, 0m, DescontoHelper.PercentualMaximo);

        _descontoEmReais = DescontoHelper.CalcularDescontoEmReais(Subtotal, valorInformado, _tipoDesconto);
        Total = DescontoHelper.CalcularTotalLiquido(Subtotal, _descontoEmReais);
        TotalItens = ItensVenda.Count;

        if (TxtDescontoEquivalente is null) return;

        TxtDescontoEquivalente.Text = _tipoDesconto == TipoDescontoVenda.Percentual && Subtotal > 0
            ? $"Equivale a {FormattingHelper.FormatarMoeda(_descontoEmReais)}"
            : string.Empty;
    }

    private decimal ObterValorDescontoInformado()
    {
        var texto = TxtDesconto?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(texto))
            return 0m;

        if (_tipoDesconto == TipoDescontoVenda.Valor &&
            FormattingHelper.TryParseMoeda(texto, out var moeda))
            return moeda;

        if (decimal.TryParse(texto.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var numero))
            return numero;

        return 0m;
    }
    private async void BtnFinalizarVenda_Click(object sender, RoutedEventArgs e)
    {
        if (!ItensVenda.Any())
        {
            MessageBox.Show("Adicione pelo menos um produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            AtualizarTotais();
            var desconto = _descontoEmReais;
            var clienteId = CmbCliente.SelectedValue is int cid && cid > 0 ? (int?)cid : null;
            var fechamento = new FechamentoVendaView(Total) { Owner = Owner };
            if (fechamento.ShowDialog() != true || fechamento.Pagamento is null)
                return;

            var dto = new CriarVendaDto
            {
                ClienteId = clienteId,
                Desconto = desconto,
                Observacoes = TxtObservacoes.Text,
                Usuario = _sessaoService.ObterNomeUsuario(),
                FormaPagamento = fechamento.Pagamento.FormaPagamento,
                QuantidadeParcelas = fechamento.Pagamento.QuantidadeParcelas,
                ValorPago = fechamento.Pagamento.ValorPago,
                Troco = fechamento.Pagamento.Troco,
                Itens = ItensVenda.Select(i => new CriarItemVendaDto
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Desconto = i.Desconto
                }).ToList()
            };

            var venda = await _vendaService.CriarAsync(dto);

            // Exibir cupom ANTES de fechar o PDV e usar Owner da janela principal (nunca a janela que será fechada)
            await WindowHelper.ExibirCupomAsync(_serviceProvider, venda, Owner);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao finalizar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnFechar_Click(object sender, RoutedEventArgs e)
    {
        if (!ItensVenda.Any() ||
            MessageBox.Show("Cancelar esta venda?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            DialogResult = false;
            Close();
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
