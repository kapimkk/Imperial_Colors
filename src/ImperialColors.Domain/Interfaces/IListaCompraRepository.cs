using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IListaCompraRepository : IRepository<ListaCompra>
{
    Task<IEnumerable<ListaCompra>> ObterTodosComItensAsync(CancellationToken cancellationToken = default);
    Task<ListaCompra?> ObterComItensAsync(int id, CancellationToken cancellationToken = default);
    Task<ListaCompra> SalvarComItensAsync(ListaCompra lista, IReadOnlyList<ItemListaCompra> itens, CancellationToken cancellationToken = default);
}
