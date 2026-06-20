using ImperialColors.Application.DTOs;
using ImperialColors.Application.Helpers;
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

    public async Task<PaginacaoResultadoDto<VendaDto>> ObterPaginadoPorPeriodoAsync(
        DateTime inicio, DateTime fim, int pagina, int itensPorPagina, string? termoBusca = null,
        CancellationToken cancellationToken = default)
    {
        var (itens, total) = await _vendaRepository.ObterPaginadoPorPeriodoAsync(
            inicio, fim, pagina, itensPorPagina, termoBusca, cancellationToken);

        return new PaginacaoResultadoDto<VendaDto>
        {
            Itens = itens.Select(MapParaDto).ToList(),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina,
            TotalItens = total
        };
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

        PagamentoHelper.ValidarPagamento(dto.FormaPagamento, venda.Total, dto.ValorPago, dto.QuantidadeParcelas);
        var (valorPago, troco, parcelas) = PagamentoHelper.CalcularPagamento(
            dto.FormaPagamento, venda.Total, dto.ValorPago, dto.QuantidadeParcelas);

        venda.FormaPagamento = dto.FormaPagamento;
        venda.QuantidadeParcelas = parcelas;
        venda.ValorPago = valorPago;
        venda.Troco = troco;

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
        await _vendaRepository.CancelarComEstornoAsync(id);
        _logger.LogInformation("Venda cancelada com estorno de estoque: {VendaId}", id);
    }

    public async Task ExcluirFisicamenteAsync(int id)
    {
        await _vendaRepository.ExcluirFisicamenteComEstornoAsync(id);
        _logger.LogWarning("Venda excluída permanentemente do banco: {VendaId}", id);
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
        FormaPagamento = v.FormaPagamento,
        QuantidadeParcelas = v.QuantidadeParcelas,
        ValorPago = v.ValorPago,
        Troco = v.Troco,
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
