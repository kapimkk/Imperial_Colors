using ImperialColors.Application.Interfaces;
using ImperialColors.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImperialColors.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IVendaService, VendaService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
