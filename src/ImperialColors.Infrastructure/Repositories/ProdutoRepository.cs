using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using ImperialColors.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
{
    public ProdutoRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

    public async Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .FirstOrDefaultAsync(p => p.CodigoInterno == codigoInterno);
    }

    public async Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
    }

    public async Task<IEnumerable<Produto>> BuscarPorNomeAsync(string nome)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .Where(p => EF.Functions.ILike(p.Nome, $"%{nome}%"))
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync()
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .Where(p => p.QuantidadeEstoque <= p.EstoqueMinimo && p.QuantidadeEstoque > 0)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produto>> ObterSemEstoqueAsync()
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .Where(p => p.QuantidadeEstoque <= 0)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<int> ContarComEstoqueCriticoAsync(decimal limiteUnidades = 5)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .CountAsync(p => p.QuantidadeEstoque > 0 && p.QuantidadeEstoque < limiteUnidades);
    }

    public async Task<IEnumerable<Produto>> ObterComCategoriaEMarcaAsync()
    {
        await using var context = ContextFactory.CreateDbContext();
        return await ConsultaLeituraComIncludes(context)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Produto> Itens, int Total)> ObterPaginadoAsync(
        int pagina,
        int itensPorPagina,
        string? termoBusca = null,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 200);

        await using var context = ContextFactory.CreateDbContext();
        var query = ConsultaLeituraComIncludes(context);

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Nome, $"%{termo}%") ||
                p.CodigoInterno == termo ||
                (p.CodigoBarras != null && p.CodigoBarras == termo));
        }

        var total = await query.CountAsync(cancellationToken);
        var itens = await query
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task<bool> CodigoInternoExisteAsync(string codigoInterno, int? ignorarId = null)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<Produto>().IgnoreQueryFilters().AnyAsync(p =>
            p.CodigoInterno == codigoInterno &&
            (ignorarId == null || p.Id != ignorarId));
    }

    public async Task<int> ObterMaiorSequenciaCodigoInternoAsync()
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Database
            .SqlQuery<int>($"""
                SELECT COALESCE(MAX(CAST(SUBSTRING(codigo_interno FROM 2) AS INTEGER)), 0) AS "Value"
                FROM produtos
                WHERE codigo_interno ~ '^P[0-9]+$'
                """)
            .SingleAsync();
    }

    public async Task<Produto> InserirProdutoAsync(
        Produto produto,
        bool permitirRegenerarCodigoInterno,
        Func<Task<string>> obterProximoCodigoInternoAsync)
    {
        const int maxTentativas = 5;

        for (var tentativa = 0; tentativa < maxTentativas; tentativa++)
        {
            await using var context = ContextFactory.CreateDbContext();
            var dbSet = context.Set<Produto>();

            await GarantirCodigoInternoDisponivelAntesDeInserirAsync(
                produto,
                permitirRegenerarCodigoInterno,
                obterProximoCodigoInternoAsync);

            try
            {
                await dbSet.AddAsync(produto);
                await SalvarAlteracoesAsync(context);
                return produto;
            }
            catch (DomainException ex) when (
                permitirRegenerarCodigoInterno &&
                tentativa < maxTentativas - 1 &&
                DatabaseExceptionHelper.EhViolacaoUnicidadeCodigoInterno(ex))
            {
                Desanexar(context, produto);
                produto.CodigoInterno = await obterProximoCodigoInternoAsync();
            }
        }

        throw new DomainException("Não foi possível gerar um código interno único. Tente novamente.");
    }

    public override async Task<Produto?> ObterPorIdAsync(int id)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<Produto>()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    private async Task GarantirCodigoInternoDisponivelAntesDeInserirAsync(
        Produto produto,
        bool permitirRegenerarCodigoInterno,
        Func<Task<string>> obterProximoCodigoInternoAsync)
    {
        if (!await CodigoInternoExisteAsync(produto.CodigoInterno))
            return;

        if (!permitirRegenerarCodigoInterno)
            throw new DomainException("Este código interno já está em uso por outro produto.");

        produto.CodigoInterno = await obterProximoCodigoInternoAsync();

        if (await CodigoInternoExisteAsync(produto.CodigoInterno))
            throw new DomainException("Este código interno já está em uso por outro produto.");
    }

    private static IQueryable<Produto> ConsultaLeituraComIncludes(AppDbContext context)
        => context.Set<Produto>()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca);
}
