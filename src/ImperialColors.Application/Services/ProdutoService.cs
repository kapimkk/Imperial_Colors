using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImperialColors.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(
        IProdutoRepository produtoRepository,
        IMovimentacaoEstoqueRepository movimentacaoRepository,
        ILogger<ProdutoService> logger)
    {
        _produtoRepository = produtoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoDto>> ObterTodosAsync()
    {
        var produtos = await _produtoRepository.ObterComCategoriaEMarcaAsync();
        return produtos.Select(MapParaDto);
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(int id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id);
        return produto is null ? null : MapParaDto(produto);
    }

    public async Task<ProdutoDto?> ObterPorCodigoBarrasAsync(string codigoBarras)
    {
        var produto = await _produtoRepository.ObterPorCodigoBarrasAsync(codigoBarras);
        return produto is null ? null : MapParaDto(produto);
    }

    public async Task<ProdutoDto?> ObterPorCodigoInternoAsync(string codigoInterno)
    {
        var produto = await _produtoRepository.ObterPorCodigoInternoAsync(codigoInterno);
        return produto is null ? null : MapParaDto(produto);
    }

    public async Task<IEnumerable<ProdutoDto>> BuscarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await ObterTodosAsync();

        var porNome = await _produtoRepository.BuscarPorNomeAsync(termo);
        var porCodigo = await _produtoRepository.ObterPorCodigoInternoAsync(termo);
        var porBarras = await _produtoRepository.ObterPorCodigoBarrasAsync(termo);

        var resultado = porNome.ToList();
        if (porCodigo is not null && !resultado.Any(p => p.Id == porCodigo.Id))
            resultado.Add(porCodigo);
        if (porBarras is not null && !resultado.Any(p => p.Id == porBarras.Id))
            resultado.Add(porBarras);

        return resultado.Select(MapParaDto);
    }

    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto)
    {
        if (await _produtoRepository.CodigoInternoExisteAsync(dto.CodigoInterno))
            throw new DomainException($"Código interno '{dto.CodigoInterno}' já está em uso.");

        var produto = new Produto
        {
            CodigoInterno = dto.CodigoInterno,
            CodigoBarras = dto.CodigoBarras,
            Nome = dto.Nome,
            CategoriaId = dto.CategoriaId,
            MarcaId = dto.MarcaId,
            QuantidadeEstoque = dto.QuantidadeEstoque,
            EstoqueMinimo = dto.EstoqueMinimo,
            Unidade = dto.Unidade,
            Custo = dto.Custo,
            PrecoVenda = dto.PrecoVenda,
            Observacoes = dto.Observacoes
        };

        var criado = await _produtoRepository.AdicionarAsync(produto);

        if (dto.QuantidadeEstoque > 0)
        {
            await _movimentacaoRepository.AdicionarAsync(new MovimentacaoEstoque
            {
                ProdutoId = criado.Id,
                Tipo = TipoMovimentacao.Entrada,
                Quantidade = dto.QuantidadeEstoque,
                QuantidadeAnterior = 0,
                QuantidadeAtual = dto.QuantidadeEstoque,
                Motivo = "Estoque inicial"
            });
        }

        _logger.LogInformation("Produto criado: {Nome} ({CodigoInterno})", dto.Nome, dto.CodigoInterno);
        return MapParaDto(criado);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id)
            ?? throw new DomainException($"Produto com Id {id} não encontrado.");

        if (await _produtoRepository.CodigoInternoExisteAsync(dto.CodigoInterno, id))
            throw new DomainException($"Código interno '{dto.CodigoInterno}' já está em uso por outro produto.");

        produto.CodigoInterno = dto.CodigoInterno;
        produto.CodigoBarras = dto.CodigoBarras;
        produto.Nome = dto.Nome;
        produto.CategoriaId = dto.CategoriaId;
        produto.MarcaId = dto.MarcaId;
        produto.EstoqueMinimo = dto.EstoqueMinimo;
        produto.Unidade = dto.Unidade;
        produto.Custo = dto.Custo;
        produto.PrecoVenda = dto.PrecoVenda;
        produto.Observacoes = dto.Observacoes;

        var atualizado = await _produtoRepository.AtualizarAsync(produto);
        _logger.LogInformation("Produto atualizado: {Nome} ({Id})", dto.Nome, id);
        return MapParaDto(atualizado);
    }

    public async Task RemoverAsync(int id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id)
            ?? throw new DomainException($"Produto com Id {id} não encontrado.");

        await _produtoRepository.RemoverAsync(id);
        _logger.LogInformation("Produto removido: {Nome} ({Id})", produto.Nome, id);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterComEstoqueBaixoAsync()
    {
        var produtos = await _produtoRepository.ObterComEstoqueBaixoAsync();
        return produtos.Select(MapParaDto);
    }

    public async Task<IEnumerable<ProdutoDto>> ObterSemEstoqueAsync()
    {
        var produtos = await _produtoRepository.ObterSemEstoqueAsync();
        return produtos.Select(MapParaDto);
    }

    public async Task RegistrarMovimentacaoAsync(MovimentacaoEstoqueDto dto)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(dto.ProdutoId)
            ?? throw new DomainException($"Produto com Id {dto.ProdutoId} não encontrado.");

        var quantidadeAnterior = produto.QuantidadeEstoque;
        decimal quantidadeAtual;

        switch (dto.Tipo)
        {
            case TipoMovimentacao.Entrada:
                quantidadeAtual = quantidadeAnterior + dto.Quantidade;
                break;
            case TipoMovimentacao.Saida:
                if (quantidadeAnterior < dto.Quantidade)
                    throw new DomainException($"Estoque insuficiente. Disponível: {quantidadeAnterior} {produto.Unidade}");
                quantidadeAtual = quantidadeAnterior - dto.Quantidade;
                break;
            case TipoMovimentacao.Ajuste:
                quantidadeAtual = dto.Quantidade;
                break;
            default:
                throw new DomainException("Tipo de movimentação inválido.");
        }

        produto.QuantidadeEstoque = quantidadeAtual;
        await _produtoRepository.AtualizarAsync(produto);

        await _movimentacaoRepository.AdicionarAsync(new MovimentacaoEstoque
        {
            ProdutoId = dto.ProdutoId,
            Tipo = dto.Tipo,
            Quantidade = dto.Tipo == TipoMovimentacao.Ajuste ? Math.Abs(quantidadeAtual - quantidadeAnterior) : dto.Quantidade,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadeAtual = quantidadeAtual,
            Motivo = dto.Motivo,
            Usuario = dto.Usuario
        });
    }

    public async Task<string> GerarProximoCodigoInternoAsync()
    {
        var todos = await _produtoRepository.ObterTodosAsync();
        var numericos = todos
            .Select(p => p.CodigoInterno)
            .Where(c => c.StartsWith("P") && int.TryParse(c[1..], out _))
            .Select(c => int.Parse(c[1..]))
            .ToList();

        int proximo = numericos.Any() ? numericos.Max() + 1 : 1;
        return $"P{proximo:D5}";
    }

    private static ProdutoDto MapParaDto(Produto p) => new()
    {
        Id = p.Id,
        CodigoInterno = p.CodigoInterno,
        CodigoBarras = p.CodigoBarras,
        Nome = p.Nome,
        CategoriaId = p.CategoriaId,
        CategoriaNome = p.Categoria?.Nome,
        MarcaId = p.MarcaId,
        MarcaNome = p.Marca?.Nome,
        QuantidadeEstoque = p.QuantidadeEstoque,
        EstoqueMinimo = p.EstoqueMinimo,
        Unidade = p.Unidade,
        Custo = p.Custo,
        PrecoVenda = p.PrecoVenda,
        Observacoes = p.Observacoes
    };
}
