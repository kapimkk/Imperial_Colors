using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImperialColors.Infrastructure.Repositories;

public class VendaRepository : RepositoryBase<Venda>, IVendaRepository
{
    public VendaRepository(AppDbContext context) : base(context) { }

    public async Task<Venda?> ObterComItensAsync(int id)
        => await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id && v.Ativo);

    public async Task<IEnumerable<Venda>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
        => await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .Where(v => v.Ativo && v.DataVenda >= inicio && v.DataVenda <= fim && v.Status == StatusVenda.Finalizada)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

    public async Task<IEnumerable<Venda>> ObterPorClienteAsync(int clienteId)
        => await _dbSet
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .Where(v => v.Ativo && v.ClienteId == clienteId && v.Status == StatusVenda.Finalizada)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

    public async Task<decimal> ObterTotalVendasDiaAsync(DateTime data)
        => await _dbSet
            .Where(v => v.Ativo && v.Status == StatusVenda.Finalizada &&
                   v.DataVenda.Date == data.Date)
            .SumAsync(v => v.Total);

    public async Task<decimal> ObterTotalVendasMesAsync(int ano, int mes)
        => await _dbSet
            .Where(v => v.Ativo && v.Status == StatusVenda.Finalizada &&
                   v.DataVenda.Year == ano && v.DataVenda.Month == mes)
            .SumAsync(v => v.Total);

    public async Task<string> GerarNumeroVendaAsync()
    {
        var hoje = DateTime.Today;
        var prefixo = hoje.ToString("yyyyMMdd");
        var ultimaVenda = await _dbSet
            .Where(v => v.NumeroVenda.StartsWith(prefixo))
            .OrderByDescending(v => v.NumeroVenda)
            .FirstOrDefaultAsync();

        int sequencial = 1;
        if (ultimaVenda is not null)
        {
            var partes = ultimaVenda.NumeroVenda.Split('-');
            if (partes.Length == 2 && int.TryParse(partes[1], out int seq))
                sequencial = seq + 1;
        }

        return $"{prefixo}-{sequencial:D4}";
    }

    public async Task<IEnumerable<object>> ObterProdutosMaisVendidosAsync(DateTime inicio, DateTime fim, int top = 10)
    {
        var resultado = await _context.ItensVenda
            .Include(i => i.Produto)
            .Include(i => i.Venda)
            .Where(i => i.Ativo && i.Venda.Status == StatusVenda.Finalizada &&
                   i.Venda.DataVenda >= inicio && i.Venda.DataVenda <= fim)
            .GroupBy(i => new { i.ProdutoId, i.Produto.Nome })
            .Select(g => new
            {
                ProdutoId = g.Key.ProdutoId,
                NomeProduto = g.Key.Nome,
                QuantidadeTotal = g.Sum(i => i.Quantidade),
                TotalVendido = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(r => r.QuantidadeTotal)
            .Take(top)
            .ToListAsync();

        return resultado.Cast<object>();
    }

    public override async Task<IEnumerable<Venda>> ObterTodosAsync()
        => await _dbSet
            .Include(v => v.Cliente)
            .Where(v => v.Ativo)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
}
