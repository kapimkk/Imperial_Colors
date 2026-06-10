using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
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

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public PDVView(IProdutoService produtoService, IVendaService vendaService,
                   IClienteService clienteService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = this;
        _produtoService = produtoService;
        _vendaService = vendaService;
        _clienteService = clienteService;
        _serviceProvider = serviceProvider;
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
        var termo = TxtBuscaProduto.Text.Trim();
        if (termo.Length < 2)
        {
            PainelResultados.Visibility = Visibility.Collapsed;
            return;
        }

        _produtosEncontrados = (await _produtoService.BuscarAsync(termo)).ToList();
        if (_produtosEncontrados.Any())
        {
            LstResultados.ItemsSource = _produtosEncontrados;
            PainelResultados.Visibility = Visibility.Visible;
        }
        else
        {
            PainelResultados.Visibility = Visibility.Collapsed;
        }
    }

    private void TxtBuscaProduto_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AdicionarProdutoBuscado();
        if (e.Key == Key.Escape) PainelResultados.Visibility = Visibility.Collapsed;
    }

    private void BtnAdicionarProduto_Click(object sender, RoutedEventArgs e) => AdicionarProdutoBuscado();

    private void AdicionarProdutoBuscado()
    {
        if (_produtosEncontrados.Count == 1)
        {
            AdicionarItemVenda(_produtosEncontrados[0]);
        }
        else if (LstResultados.SelectedItem is ProdutoDto selecionado)
        {
            AdicionarItemVenda(selecionado);
        }
    }

    private void LstResultados_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    private void LstResultados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LstResultados.SelectedItem is ProdutoDto produto)
        {
            AdicionarItemVenda(produto);
            PainelResultados.Visibility = Visibility.Collapsed;
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

        TxtBuscaProduto.Clear();
        PainelResultados.Visibility = Visibility.Collapsed;
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

    private void AtualizarTotais()
    {
        Subtotal = ItensVenda.Sum(i => i.Subtotal);
        decimal.TryParse(TxtDesconto?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal desconto);
        Total = Math.Max(0, Subtotal - desconto);
        TotalItens = ItensVenda.Count;
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
            decimal.TryParse(TxtDesconto.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal desconto);
            var clienteId = CmbCliente.SelectedValue is int cid && cid > 0 ? (int?)cid : null;

            var dto = new CriarVendaDto
            {
                ClienteId = clienteId,
                Desconto = desconto,
                Observacoes = TxtObservacoes.Text,
                Usuario = "Administrador",
                Itens = ItensVenda.Select(i => new CriarItemVendaDto
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Desconto = i.Desconto
                }).ToList()
            };

            var venda = await _vendaService.CriarAsync(dto);

            var cupom = (CupomView)_serviceProvider.GetService(typeof(CupomView))!;
            cupom.Owner = this;
            cupom.InicializarVenda(venda);

            DialogResult = true;
            Close();
            cupom.ShowDialog();
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
