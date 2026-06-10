using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IVendaRepository : IRepository<Venda>
{
    Task<Venda?> ObterComItensAsync(int id);
    Task<IEnumerable<Venda>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<Venda>> ObterPorClienteAsync(int clienteId);
    Task<decimal> ObterTotalVendasDiaAsync(DateTime data);
    Task<decimal> ObterTotalVendasMesAsync(int ano, int mes);
    Task<string> GerarNumeroVendaAsync();
    Task<IEnumerable<object>> ObterProdutosMaisVendidosAsync(DateTime inicio, DateTime fim, int top = 10);
}
