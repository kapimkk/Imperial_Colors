using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImperialColors.Infrastructure.Repositories;

public class VendaExternaRepository : RepositoryBase<VendaExterna>, IVendaExternaRepository
{
    public VendaExternaRepository(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<VendaExternaRepository> logger)
        : base(contextFactory, logger) { }

    public async Task<IEnumerable<VendaExterna>> ObterTodosComItensAsync(CancellationToken cancellationToken = default)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<VendaExterna>()
            .AsNoTracking()
            .Include(v => v.Itens)
            .OrderByDescending(v => v.DataVenda)
            .ThenByDescending(v => v.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<VendaExterna?> ObterComItensAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = ContextFactory.CreateDbContext();
        return await context.Set<VendaExterna>()
            .AsNoTracking()
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<string> GerarNumeroVendaExternaAsync(CancellationToken cancellationToken = default)
    {
        await using var context = ContextFactory.CreateDbContext();
        var hoje = DateTime.Today;
        var prefixo = $"EXT-{hoje:yyyyMMdd}";
        var ultima = await context.Set<VendaExterna>()
            .Where(v => v.NumeroVendaExterna.StartsWith(prefixo))
            .OrderByDescending(v => v.NumeroVendaExterna)
            .FirstOrDefaultAsync(cancellationToken);

        var sequencial = 1;
        if (ultima is not null)
        {
            var partes = ultima.NumeroVendaExterna.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var seq))
                sequencial = seq + 1;
        }

        return $"{prefixo}-{sequencial:D4}";
    }

    public async Task<VendaExterna> RegistrarTransacionalAsync(
        VendaExterna venda,
        IReadOnlyList<ItemVendaExterna> itens,
        string? usuario,
        CancellationToken cancellationToken = default)
    {
        await using var context = ContextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            venda.Usuario = usuario;
            venda.Itens = itens.ToList();
            venda.CalcularTotais();

            foreach (var item in itens)
            {
                item.CalcularSubtotal();
                if (item.Quantidade <= 0)
                    throw new DomainException($"Quantidade inválida para '{item.NomeProduto}'.");
                if (item.PrecoUnitario < 0)
                    throw new DomainException($"Preço unitário inválido para '{item.NomeProduto}'.");
            }

            await context.Set<VendaExterna>().AddAsync(venda, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var item in itens.Where(i => i.ProdutoId.HasValue))
            {
                var produto = await context.Set<Produto>()
                    .FirstOrDefaultAsync(p => p.Id == item.ProdutoId!.Value, cancellationToken)
                    ?? throw new DomainException($"Produto (Id={item.ProdutoId}) não encontrado.");

                if (produto.QuantidadeEstoque < item.Quantidade)
                    throw new DomainException(
                        $"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.QuantidadeEstoque}.");

                var qtdAnterior = produto.QuantidadeEstoque;
                produto.QuantidadeEstoque -= item.Quantidade;

                context.Set<MovimentacaoEstoque>().Add(new MovimentacaoEstoque
                {
                    ProdutoId = produto.Id,
                    Tipo = TipoMovimentacao.Saida,
                    Quantidade = item.Quantidade,
                    QuantidadeAnterior = qtdAnterior,
                    QuantidadeAtual = produto.QuantidadeEstoque,
                    Motivo = $"Venda externa #{venda.NumeroVendaExterna}",
                    VendaExternaId = venda.Id,
                    Usuario = usuario
                });
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return venda;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
