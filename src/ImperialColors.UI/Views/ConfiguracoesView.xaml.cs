using ImperialColors.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImperialColors.UI.Views;

public partial class ConfiguracoesView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    public ConfiguracoesView(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        CarregarConfiguracoes();
    }

    private void CarregarConfiguracoes()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonDocument.Parse(json);
                if (config.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                    cs.TryGetProperty("DefaultConnection", out var conn))
                    TxtConnectionString.Text = conn.GetString();
            }
        }
        catch { }
    }

    private async void BtnTestarConexao_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var ctx = _serviceProvider.GetRequiredService<AppDbContext>();
            var pode = await ctx.Database.CanConnectAsync();
            StatusConexao.Visibility = Visibility.Visible;
            if (pode)
            {
                StatusConexao.Background = new SolidColorBrush(Color.FromRgb(212, 237, 218));
                TxtStatusConexao.Text = "✓ Conexão com o banco de dados estabelecida com sucesso!";
                TxtStatusConexao.Foreground = new SolidColorBrush(Color.FromRgb(21, 87, 36));
            }
            else
            {
                StatusConexao.Background = new SolidColorBrush(Color.FromRgb(248, 215, 218));
                TxtStatusConexao.Text = "✗ Não foi possível conectar ao banco de dados.";
                TxtStatusConexao.Foreground = new SolidColorBrush(Color.FromRgb(114, 28, 36));
            }
        }
        catch (Exception ex)
        {
            StatusConexao.Visibility = Visibility.Visible;
            StatusConexao.Background = new SolidColorBrush(Color.FromRgb(248, 215, 218));
            TxtStatusConexao.Text = $"✗ Erro: {ex.Message}";
            TxtStatusConexao.Foreground = new SolidColorBrush(Color.FromRgb(114, 28, 36));
        }
    }

    private void BtnSalvarConexao_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = new
            {
                ConnectionStrings = new { DefaultConnection = TxtConnectionString.Text }
            };
            File.WriteAllText(_configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            MessageBox.Show("Configurações salvas. Reinicie o sistema para aplicar.", "Salvo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
