using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context) { }

    public async Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno)
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.CodigoInterno == codigoInterno && p.Ativo);

    public async Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras)
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras && p.Ativo);

    public async Task<IEnumerable<Produto>> BuscarPorNomeAsync(string nome)
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Ativo && EF.Functions.ILike(p.Nome, $"%{nome}%"))
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync()
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Ativo && p.QuantidadeEstoque <= p.EstoqueMinimo && p.QuantidadeEstoque > 0)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Produto>> ObterSemEstoqueAsync()
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Ativo && p.QuantidadeEstoque <= 0)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Produto>> ObterComCategoriaEMarcaAsync()
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task<bool> CodigoInternoExisteAsync(string codigoInterno, int? ignorarId = null)
        => await _dbSet.AnyAsync(p => p.CodigoInterno == codigoInterno && p.Ativo && (ignorarId == null || p.Id != ignorarId));

    public override async Task<IEnumerable<Produto>> ObterTodosAsync()
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public override async Task<Produto?> ObterPorIdAsync(int id)
        => await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
}
