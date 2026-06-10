using ImperialColors.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ImperialColors.UI.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DispatcherTimer _relogio;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        _relogio = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _relogio.Tick += (s, e) => TxtHora.Text = DateTime.Now.ToString("HH:mm:ss");
        _relogio.Start();

        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        TxtTituloPagina.Text = "Dashboard";
        var vm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        ConteudoPrincipal.Content = new DashboardView(vm);
        _ = vm.CarregarDados();
    }

    private void BtnDashboard_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Dashboard";
        var vm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        ConteudoPrincipal.Content = new DashboardView(vm);
        _ = vm.CarregarDados();
    }

    private void BtnEstoque_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Controle de Estoque";
        var vm = _serviceProvider.GetRequiredService<ProdutoViewModel>();
        ConteudoPrincipal.Content = new EstoqueView(vm);
        _ = vm.CarregarAsync();
    }

    private void BtnVendas_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Histórico de Vendas";
        var vm = _serviceProvider.GetRequiredService<VendaViewModel>();
        ConteudoPrincipal.Content = new VendasView(vm);
        _ = vm.CarregarAsync();
    }

    private void BtnPDV_Click(object sender, RoutedEventArgs e)
    {
        var pdv = _serviceProvider.GetRequiredService<PDVView>();
        pdv.Owner = this;
        pdv.ShowDialog();
    }

    private void BtnClientes_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Clientes";
        var vm = _serviceProvider.GetRequiredService<ClienteViewModel>();
        ConteudoPrincipal.Content = new ClientesView(vm);
        _ = vm.CarregarAsync();
    }

    private void BtnMercadorias_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Mercadorias / Fornecedores";
        var vm = _serviceProvider.GetRequiredService<FornecedorViewModel>();
        ConteudoPrincipal.Content = new MercadoriasView(vm);
        _ = vm.CarregarAsync();
    }

    private void BtnRelatorios_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Relatórios";
        ConteudoPrincipal.Content = new RelatoriosView(_serviceProvider);
    }

    private void BtnConfiguracoes_Click(object sender, RoutedEventArgs e)
    {
        TxtTituloPagina.Text = "Configurações";
        ConteudoPrincipal.Content = new ConfiguracoesView(_serviceProvider);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void BtnFechar_Click(object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
}
