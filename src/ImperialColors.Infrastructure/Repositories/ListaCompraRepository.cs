using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class ListaCompraRepository : RepositoryBase<ListaCompra>, IRepository<ListaCompra>
{
    public ListaCompraRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<ListaCompra>> ObterTodosAsync()
        => await _dbSet
            .Include(l => l.Fornecedor)
            .Include(l => l.Itens).ThenInclude(i => i.Produto)
            .Where(l => l.Ativo)
            .OrderByDescending(l => l.CriadoEm)
            .ToListAsync();

    public override async Task<ListaCompra?> ObterPorIdAsync(int id)
        => await _dbSet
            .Include(l => l.Fornecedor)
            .Include(l => l.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(l => l.Id == id && l.Ativo);
}
