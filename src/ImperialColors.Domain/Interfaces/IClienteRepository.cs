using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IEnumerable<Cliente>> BuscarPorNomeAsync(string nome);
    Task<Cliente?> ObterComVendasAsync(int id);
}
