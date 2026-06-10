using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImperialColors.Application.Services;

public class VendaService : IVendaService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
    private readonly ILogger<VendaService> _logger;

    public VendaService(
        IVendaRepository vendaRepository,
        IProdutoRepository produtoRepository,
        IMovimentacaoEstoqueRepository movimentacaoRepository,
        ILogger<VendaService> logger)
    {
        _vendaRepository = vendaRepository;
        _produtoRepository = produtoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VendaDto>> ObterTodosAsync()
    {
        var vendas = await _vendaRepository.ObterTodosAsync();
        return vendas.Select(MapParaDto);
    }

    public async Task<VendaDto?> ObterPorIdAsync(int id)
    {
        var venda = await _vendaRepository.ObterPorIdAsync(id);
        return venda is null ? null : MapParaDto(venda);
    }

    public async Task<VendaDto?> ObterComItensAsync(int id)
    {
        var venda = await _vendaRepository.ObterComItensAsync(id);
        return venda is null ? null : MapParaDto(venda);
    }

    public async Task<IEnumerable<VendaDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var vendas = await _vendaRepository.ObterPorPeriodoAsync(inicio, fim);
        return vendas.Select(MapParaDto);
    }

    public async Task<IEnumerable<VendaDto>> ObterPorClienteAsync(int clienteId)
    {
        var vendas = await _vendaRepository.ObterPorClienteAsync(clienteId);
        return vendas.Select(MapParaDto);
    }

    public async Task<VendaDto> CriarAsync(CriarVendaDto dto)
    {
        if (!dto.Itens.Any())
            throw new DomainException("A venda deve ter pelo menos um item.");

        foreach (var item in dto.Itens)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(item.ProdutoId)
                ?? throw new DomainException($"Produto com Id {item.ProdutoId} não encontrado.");

            if (produto.QuantidadeEstoque < item.Quantidade)
                throw new DomainException($"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.QuantidadeEstoque} {produto.Unidade}");
        }

        var numeroVenda = await _vendaRepository.GerarNumeroVendaAsync();

        var venda = new Venda
        {
            NumeroVenda = numeroVenda,
            ClienteId = dto.ClienteId,
            Status = StatusVenda.Finalizada,
            Desconto = dto.Desconto,
            Observacoes = dto.Observacoes,
            Usuario = dto.Usuario,
            DataVenda = DateTime.Now,
            Itens = dto.Itens.Select(i =>
            {
                var item = new ItemVenda
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Desconto = i.Desconto
                };
                item.CalcularSubtotal();
                return item;
            }).ToList()
        };

        venda.CalcularTotais();

        var vendaCriada = await _vendaRepository.AdicionarAsync(venda);

        foreach (var item in dto.Itens)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(item.ProdutoId)!;
            var quantidadeAnterior = produto!.QuantidadeEstoque;
            produto.QuantidadeEstoque -= item.Quantidade;
            await _produtoRepository.AtualizarAsync(produto);

            await _movimentacaoRepository.AdicionarAsync(new MovimentacaoEstoque
            {
                ProdutoId = item.ProdutoId,
                Tipo = TipoMovimentacao.Saida,
                Quantidade = item.Quantidade,
                QuantidadeAnterior = quantidadeAnterior,
                QuantidadeAtual = produto.QuantidadeEstoque,
                Motivo = $"Venda #{numeroVenda}",
                Usuario = dto.Usuario,
                VendaId = vendaCriada.Id
            });
        }

        _logger.LogInformation("Venda criada: {NumeroVenda} - Total: {Total}", numeroVenda, venda.Total);

        var vendaCompleta = await _vendaRepository.ObterComItensAsync(vendaCriada.Id);
        return MapParaDto(vendaCompleta!);
    }

    public async Task<VendaDto> FinalizarAsync(int id)
    {
        var venda = await _vendaRepository.ObterPorIdAsync(id)
            ?? throw new DomainException($"Venda com Id {id} não encontrada.");

        if (venda.Status != StatusVenda.Aberta)
            throw new DomainException("Apenas vendas abertas podem ser finalizadas.");

        venda.Status = StatusVenda.Finalizada;
        await _vendaRepository.AtualizarAsync(venda);

        return MapParaDto(venda);
    }

    public async Task CancelarAsync(int id)
    {
        var venda = await _vendaRepository.ObterComItensAsync(id)
            ?? throw new DomainException($"Venda com Id {id} não encontrada.");

        if (venda.Status == StatusVenda.Cancelada)
            throw new DomainException("Venda já está cancelada.");

        if (venda.Status == StatusVenda.Finalizada)
        {
            foreach (var item in venda.Itens)
            {
                var produto = await _produtoRepository.ObterPorIdAsync(item.ProdutoId);
                if (produto is not null)
                {
                    var qtdAnterior = produto.QuantidadeEstoque;
                    produto.QuantidadeEstoque += item.Quantidade;
                    await _produtoRepository.AtualizarAsync(produto);

                    await _movimentacaoRepository.AdicionarAsync(new MovimentacaoEstoque
                    {
                        ProdutoId = item.ProdutoId,
                        Tipo = TipoMovimentacao.Entrada,
                        Quantidade = item.Quantidade,
                        QuantidadeAnterior = qtdAnterior,
                        QuantidadeAtual = produto.QuantidadeEstoque,
                        Motivo = $"Cancelamento venda #{venda.NumeroVenda}",
                        VendaId = id
                    });
                }
            }
        }

        venda.Status = StatusVenda.Cancelada;
        await _vendaRepository.AtualizarAsync(venda);
    }

    public async Task<decimal> ObterTotalVendasDiaAsync()
        => await _vendaRepository.ObterTotalVendasDiaAsync(DateTime.Today);

    public async Task<decimal> ObterTotalVendasMesAsync()
        => await _vendaRepository.ObterTotalVendasMesAsync(DateTime.Now.Year, DateTime.Now.Month);

    public async Task<IEnumerable<object>> ObterProdutosMaisVendidosAsync(DateTime inicio, DateTime fim, int top = 10)
        => await _vendaRepository.ObterProdutosMaisVendidosAsync(inicio, fim, top);

    private static VendaDto MapParaDto(Venda v) => new()
    {
        Id = v.Id,
        NumeroVenda = v.NumeroVenda,
        ClienteId = v.ClienteId,
        ClienteNome = v.Cliente?.Nome,
        Status = v.Status,
        Subtotal = v.Subtotal,
        Desconto = v.Desconto,
        Total = v.Total,
        Observacoes = v.Observacoes,
        Usuario = v.Usuario,
        DataVenda = v.DataVenda,
        Itens = v.Itens?.Select(i => new ItemVendaDto
        {
            Id = i.Id,
            ProdutoId = i.ProdutoId,
            NomeProduto = i.Produto?.Nome ?? string.Empty,
            CodigoInterno = i.Produto?.CodigoInterno,
            Quantidade = i.Quantidade,
            PrecoUnitario = i.PrecoUnitario,
            Desconto = i.Desconto,
            Subtotal = i.Subtotal,
            Unidade = i.Produto?.Unidade ?? "UN"
        }).ToList() ?? new()
    };
}
