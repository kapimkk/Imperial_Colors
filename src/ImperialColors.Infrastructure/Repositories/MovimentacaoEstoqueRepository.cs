using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class MovimentacaoEstoqueRepository : RepositoryBase<MovimentacaoEstoque>, IMovimentacaoEstoqueRepository
{
    public MovimentacaoEstoqueRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorProdutoAsync(int produtoId)
        => await _dbSet
            .Include(m => m.Produto)
            .Where(m => m.Ativo && m.ProdutoId == produtoId)
            .OrderByDescending(m => m.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
        => await _dbSet
            .Include(m => m.Produto)
            .Where(m => m.Ativo && m.CriadoEm >= inicio && m.CriadoEm <= fim)
            .OrderByDescending(m => m.CriadoEm)
            .ToListAsync();
}
