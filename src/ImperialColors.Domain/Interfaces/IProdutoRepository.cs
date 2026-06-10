using ImperialColors.Domain.Entities;

namespace ImperialColors.Domain.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno);
    Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras);
    Task<IEnumerable<Produto>> BuscarPorNomeAsync(string nome);
    Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync();
    Task<IEnumerable<Produto>> ObterSemEstoqueAsync();
    Task<IEnumerable<Produto>> ObterComCategoriaEMarcaAsync();
    Task<bool> CodigoInternoExisteAsync(string codigoInterno, int? ignorarId = null);
    Task<int> ObterMaiorSequenciaCodigoInternoAsync();
    Task<Produto> InserirProdutoAsync(Produto produto, bool permitirRegenerarCodigoInterno, Func<Task<string>> obterProximoCodigoInternoAsync);
    Task<(IReadOnlyList<Produto> Itens, int Total)> ObterPaginadoAsync(int pagina, int itensPorPagina, string? termoBusca = null, CancellationToken cancellationToken = default);
}
