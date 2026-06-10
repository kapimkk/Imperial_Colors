using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImperialColors.Application.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly ILogger<FornecedorService> _logger;

    public FornecedorService(IFornecedorRepository fornecedorRepository, ILogger<FornecedorService> logger)
    {
        _fornecedorRepository = fornecedorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FornecedorDto>> ObterTodosAsync()
    {
        var fornecedores = await _fornecedorRepository.ObterTodosAsync();
        return fornecedores.Select(MapParaDto);
    }

    public async Task<FornecedorDto?> ObterPorIdAsync(int id)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);
        return fornecedor is null ? null : MapParaDto(fornecedor);
    }

    public async Task<IEnumerable<FornecedorDto>> BuscarAsync(string nome)
    {
        var fornecedores = string.IsNullOrWhiteSpace(nome)
            ? await _fornecedorRepository.ObterTodosAsync()
            : await _fornecedorRepository.BuscarPorNomeAsync(nome);
        return fornecedores.Select(MapParaDto);
    }

    public async Task<FornecedorDto> CriarAsync(FornecedorDto dto)
    {
        var fornecedor = MapParaEntidade(dto);
        var criado = await _fornecedorRepository.AdicionarAsync(fornecedor);
        return MapParaDto(criado);
    }

    public async Task<FornecedorDto> AtualizarAsync(int id, FornecedorDto dto)
    {
        var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id)
            ?? throw new DomainException($"Fornecedor com Id {id} não encontrado.");

        fornecedor.Nome = dto.Nome;
        fornecedor.Telefone = dto.Telefone;
        fornecedor.WhatsApp = dto.WhatsApp;
        fornecedor.Email = dto.Email;
        fornecedor.Cep = dto.Cep;
        fornecedor.Logradouro = dto.Logradouro;
        fornecedor.Numero = dto.Numero;
        fornecedor.Complemento = dto.Complemento;
        fornecedor.Bairro = dto.Bairro;
        fornecedor.Cidade = dto.Cidade;
        fornecedor.Estado = dto.Estado;
        fornecedor.Observacoes = dto.Observacoes;

        var atualizado = await _fornecedorRepository.AtualizarAsync(fornecedor);
        return MapParaDto(atualizado);
    }

    public async Task RemoverAsync(int id)
    {
        if (!await _fornecedorRepository.ExisteAsync(id))
            throw new DomainException($"Fornecedor com Id {id} não encontrado.");
        await _fornecedorRepository.RemoverAsync(id);
    }

    private static FornecedorDto MapParaDto(Fornecedor f) => new()
    {
        Id = f.Id, Nome = f.Nome, Telefone = f.Telefone, WhatsApp = f.WhatsApp,
        Email = f.Email, Cep = f.Cep, Logradouro = f.Logradouro, Numero = f.Numero,
        Complemento = f.Complemento, Bairro = f.Bairro, Cidade = f.Cidade,
        Estado = f.Estado, Observacoes = f.Observacoes
    };

    private static Fornecedor MapParaEntidade(FornecedorDto dto) => new()
    {
        Nome = dto.Nome, Telefone = dto.Telefone, WhatsApp = dto.WhatsApp,
        Email = dto.Email, Cep = dto.Cep, Logradouro = dto.Logradouro, Numero = dto.Numero,
        Complemento = dto.Complemento, Bairro = dto.Bairro, Cidade = dto.Cidade,
        Estado = dto.Estado, Observacoes = dto.Observacoes
    };
}
