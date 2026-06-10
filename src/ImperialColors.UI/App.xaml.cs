using DotNetEnv;
using ImperialColors.Application.Extensions;
using ImperialColors.Infrastructure.Data;
using ImperialColors.Infrastructure.Extensions;
using ImperialColors.UI.Services;
using ImperialColors.UI.ViewModels;
using ImperialColors.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ImperialColors.UI;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Permite que DateTime com Kind=Local seja aceito pelo Npgsql sem precisar converter para UTC manualmente
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        CarregarArquivoEnv();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                var connectionString = ObterStringConexao();

                services.AddInfrastructure(connectionString);
                services.AddApplication();

                services.AddSingleton<IRelatorioService, RelatorioService>();

                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProdutoViewModel>();
                services.AddTransient<VendaViewModel>();
                services.AddTransient<ClienteViewModel>();
                services.AddTransient<FornecedorViewModel>();

                services.AddTransient<ImperialColors.UI.Views.MainWindow>();
                services.AddTransient<PDVView>();
                services.AddTransient<ProdutoFormView>();
                services.AddTransient<MovimentacaoEstoqueView>();
                services.AddTransient<ClienteFormView>();
                services.AddTransient<FornecedorFormView>();
                services.AddTransient<CupomView>();

                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        await _host.StartAsync();

        try
        {
            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var resultado = System.Windows.MessageBox.Show(
                $"Erro ao conectar com o banco de dados:\n\n{ex.Message}\n\n" +
                "Verifique o arquivo .env na pasta do sistema e tente novamente.\n\n" +
                "Deseja continuar mesmo assim?",
                "Erro de Banco de Dados",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Error);

            if (resultado == System.Windows.MessageBoxResult.No)
            {
                Shutdown();
                return;
            }
        }

        var mainWindow = _host.Services.GetRequiredService<ImperialColors.UI.Views.MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static void CarregarArquivoEnv()
    {
        // Tenta carregar da pasta do executável primeiro, depois da raiz do projeto
        var caminhos = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", ".env"),
        };

        foreach (var caminho in caminhos)
        {
            var caminhoCompleto = Path.GetFullPath(caminho);
            if (File.Exists(caminhoCompleto))
            {
                Env.Load(caminhoCompleto);
                return;
            }
        }
    }

    private static string ObterStringConexao()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "imperial_colors";
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    }
}
