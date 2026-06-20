using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IVendaExternaRepository : IRepository<VendaExterna>
{
    Task<IEnumerable<VendaExterna>> ObterTodosComItensAsync(CancellationToken cancellationToken = default);
    Task<VendaExterna?> ObterComItensAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GerarNumeroVendaExternaAsync(CancellationToken cancellationToken = default);
    Task<VendaExterna> RegistrarTransacionalAsync(
        VendaExterna venda,
        IReadOnlyList<ItemVendaExterna> itens,
        string? usuario,
        CancellationToken cancellationToken = default);
}
