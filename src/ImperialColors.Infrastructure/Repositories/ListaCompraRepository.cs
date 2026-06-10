using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class ListaCompraRepository : RepositoryBase<ListaCompra>, IRepository<ListaCompra>
{
    public ListaCompraRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

    public override async Task<IEnumerable<ListaCompra>> ObterTodosAsync()
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<ListaCompra>()
            .AsNoTracking()
            .Include(l => l.Fornecedor)
            .Include(l => l.Itens).ThenInclude(i => i.Produto)
            .OrderByDescending(l => l.CriadoEm)
            .ToListAsync();
    }

    public override async Task<ListaCompra?> ObterPorIdAsync(int id)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<ListaCompra>()
            .Include(l => l.Fornecedor)
            .Include(l => l.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(l => l.Id == id);
    }
}
