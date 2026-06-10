using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using ImperialColors.Infrastructure.Repositories;
using ImperialColors.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImperialColors.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<IProdutoRepository, ProdutoRepository>();
        services.AddSingleton<IVendaRepository, VendaRepository>();
        services.AddSingleton<IClienteRepository, ClienteRepository>();
        services.AddSingleton<IFornecedorRepository, FornecedorRepository>();
        services.AddSingleton<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
        services.AddSingleton<IRepository<Categoria>, CategoriaRepository>();
        services.AddSingleton<IRepository<Marca>, MarcaRepository>();
        services.AddSingleton<IRepository<ListaCompra>, ListaCompraRepository>();
        services.AddSingleton<IRepository<ItemListaCompra>, RepositoryBase<ItemListaCompra>>();
        services.AddSingleton<IUsuarioRepository, UsuarioRepository>();

        services.AddSingleton<IPrinterService, PrinterService>();
        services.AddSingleton<ILocalConfigService, LocalConfigService>();

        return services;
    }
}
