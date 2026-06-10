using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cliente>> BuscarPorNomeAsync(string nome)
        => await _dbSet
            .Where(c => c.Ativo && EF.Functions.ILike(c.Nome, $"%{nome}%"))
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public async Task<Cliente?> ObterComVendasAsync(int id)
        => await _dbSet
            .Include(c => c.Vendas).ThenInclude(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
}
