using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class FornecedorRepository : RepositoryBase<Fornecedor>, IFornecedorRepository
{
    public FornecedorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Fornecedor>> BuscarPorNomeAsync(string nome)
        => await _dbSet
            .Where(f => f.Ativo && EF.Functions.ILike(f.Nome, $"%{nome}%"))
            .OrderBy(f => f.Nome)
            .ToListAsync();
}
