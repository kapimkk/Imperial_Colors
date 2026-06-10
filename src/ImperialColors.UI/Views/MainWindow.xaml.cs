using ImperialColors.UI.Helpers;

using ImperialColors.UI.Services;

using ImperialColors.UI.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Input;

using System.Windows.Threading;



namespace ImperialColors.UI.Views;



public partial class MainWindow : Window

{

    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ISessaoService _sessaoService;

    private readonly IAppConfigService _config;

    private readonly DispatcherTimer _relogio;

    private Button? _botaoMenuAtual;

    private IServiceScope? _escopoPagina;



    public MainWindow(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory,

        ISessaoService sessaoService, IAppConfigService config)

    {

        InitializeComponent();

        _serviceProvider = serviceProvider;

        _scopeFactory = scopeFactory;

        _sessaoService = sessaoService;

        _config = config;



        Title = $"{_config.EmpresaNome} - {_config.EmpresaSubtitulo}";

        TxtEmpresaNome.Text = _config.EmpresaNome;

        TxtEmpresaSubtitulo.Text = _config.EmpresaSubtitulo;

        LogoHelper.AplicarIconeJanela(this, _config.IconPath);

        LogoHelper.AplicarLogo(ImgLogoMenu, _config.LogoSemFundoPath, 44, 44);



        AplicarInformacoesUsuario();

        AplicarPermissoesMenu();



        _relogio = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        _relogio.Tick += (s, e) => TxtHora.Text = DateTime.Now.ToString("HH:mm:ss");

        _relogio.Start();



        NavigateToDashboard();

        Closed += (_, _) => _escopoPagina?.Dispose();

    }



    private IServiceProvider ObterServicosPagina()

    {

        _escopoPagina?.Dispose();

        _escopoPagina = _scopeFactory.CreateScope();

        return _escopoPagina.ServiceProvider;

    }



    private void AbrirModal<TWindow>() where TWindow : Window

    {

        using var escopo = _scopeFactory.CreateScope();

        if (escopo.ServiceProvider.GetRequiredService(typeof(TWindow)) is not TWindow janela)

            return;

        janela.Owner = this;

        janela.ShowDialog();

    }



    private void DefinirMenuAtivo(Button botao)

    {

        if (_botaoMenuAtual is not null)

            NavMenuHelper.SetIsActive(_botaoMenuAtual, false);



        _botaoMenuAtual = botao;

        NavMenuHelper.SetIsActive(botao, true);

    }



    private void AplicarInformacoesUsuario()

    {

        var usuario = _sessaoService.UsuarioAtual;

        TxtUsuarioLogado.Text = usuario?.NomeCompleto ?? "—";

        TxtPermissaoLogada.Text = usuario?.Permissao switch

        {

            Domain.Enums.PermissaoUsuario.Admin => "Administrador",

            Domain.Enums.PermissaoUsuario.Caixa => "Caixa",

            _ => "—"

        };

    }



    private void AplicarPermissoesMenu()

    {

        BtnRelatorios.Visibility = _sessaoService.EhAdmin ? Visibility.Visible : Visibility.Collapsed;

    }



    private void NavigateToDashboard()

    {

        DefinirMenuAtivo(BtnDashboard);

        TxtTituloPagina.Text = "Dashboard";

        var vm = ObterServicosPagina().GetRequiredService<DashboardViewModel>();

        ConteudoPrincipal.Content = new DashboardView(vm, _config);

        _ = vm.CarregarDados();

    }



    private void BtnDashboard_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnDashboard);

        TxtTituloPagina.Text = "Dashboard";

        var vm = ObterServicosPagina().GetRequiredService<DashboardViewModel>();

        ConteudoPrincipal.Content = new DashboardView(vm, _config);

        _ = vm.CarregarDados();

    }



    private void BtnEstoque_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnEstoque);

        TxtTituloPagina.Text = "Controle de Estoque";

        var vm = ObterServicosPagina().GetRequiredService<ProdutoViewModel>();

        ConteudoPrincipal.Content = new EstoqueView(vm);

        _ = vm.CarregarAsync();

    }



    private void BtnVendas_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnVendas);

        TxtTituloPagina.Text = "Histórico de Vendas";

        var vm = ObterServicosPagina().GetRequiredService<VendaViewModel>();

        ConteudoPrincipal.Content = new VendasView(vm);

        _ = vm.CarregarAsync();

    }



    private void BtnPDV_Click(object sender, RoutedEventArgs e) => AbrirModal<PDVView>();



    private void BtnClientes_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnClientes);

        TxtTituloPagina.Text = "Clientes";

        var vm = ObterServicosPagina().GetRequiredService<ClienteViewModel>();

        ConteudoPrincipal.Content = new ClientesView(vm);

        _ = vm.CarregarAsync();

    }



    private void BtnMercadorias_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnMercadorias);

        TxtTituloPagina.Text = "Mercadorias / Fornecedores";

        var vm = ObterServicosPagina().GetRequiredService<FornecedorViewModel>();

        ConteudoPrincipal.Content = new MercadoriasView(vm);

        _ = vm.CarregarAsync();

    }



    private void BtnRelatorios_Click(object sender, RoutedEventArgs e)

    {

        if (!_sessaoService.EhAdmin)

        {

            MessageBox.Show("Acesso negado. Relatórios disponíveis apenas para administradores.",

                "Permissão insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);

            return;

        }



        DefinirMenuAtivo(BtnRelatorios);

        TxtTituloPagina.Text = "Relatórios";

        ConteudoPrincipal.Content = new RelatoriosView(_serviceProvider);

    }



    private void BtnConfiguracoes_Click(object sender, RoutedEventArgs e)

    {

        DefinirMenuAtivo(BtnConfiguracoes);

        TxtTituloPagina.Text = "Configurações";

        ConteudoPrincipal.Content = new ConfiguracoesView(_serviceProvider, _sessaoService);

    }



    private void BtnLogout_Click(object sender, RoutedEventArgs e)

    {

        if (MessageBox.Show("Deseja sair da sua conta?", "Confirmar logout",

                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)

            return;



        _sessaoService.EncerrarSessao();

        Hide();



        using var escopoLogin = _scopeFactory.CreateScope();

        var login = escopoLogin.ServiceProvider.GetRequiredService<LoginView>();

        if (login.ShowDialog() == true)

        {

            AplicarInformacoesUsuario();

            AplicarPermissoesMenu();

            NavigateToDashboard();

            Show();

        }

        else

        {

            System.Windows.Application.Current.Shutdown();

        }

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


