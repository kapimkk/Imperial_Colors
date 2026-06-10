using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<IEnumerable<Fornecedor>> BuscarPorNomeAsync(string nome);
}
