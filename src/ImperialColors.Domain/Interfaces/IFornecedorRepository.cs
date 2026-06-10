using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<IEnumerable<Fornecedor>> BuscarPorNomeAsync(string nome);
    Task<(IReadOnlyList<Fornecedor> Itens, int Total)> ObterPaginadoAsync(
        int pagina, int itensPorPagina, string? termoBusca = null, CancellationToken cancellationToken = default);
}
