using ImperialColors.Application.DTOs;
using ImperialColors.Application.Interfaces;
using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Exceptions;
using ImperialColors.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImperialColors.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger)
    {
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClienteDto>> ObterTodosAsync()
    {
        var clientes = await _clienteRepository.ObterTodosAsync();
        return clientes.Select(MapParaDto);
    }

    public async Task<ClienteDto?> ObterPorIdAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        return cliente is null ? null : MapParaDto(cliente);
    }

    public async Task<IEnumerable<ClienteDto>> BuscarAsync(string nome)
    {
        var clientes = string.IsNullOrWhiteSpace(nome)
            ? await _clienteRepository.ObterTodosAsync()
            : await _clienteRepository.BuscarPorNomeAsync(nome);
        return clientes.Select(MapParaDto);
    }

    public async Task<ClienteDto> CriarAsync(ClienteDto dto)
    {
        var cliente = MapParaEntidade(dto);
        var criado = await _clienteRepository.AdicionarAsync(cliente);
        _logger.LogInformation("Cliente criado: {Nome}", dto.Nome);
        return MapParaDto(criado);
    }

    public async Task<ClienteDto> AtualizarAsync(int id, ClienteDto dto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id)
            ?? throw new DomainException($"Cliente com Id {id} não encontrado.");

        cliente.Nome = dto.Nome;
        cliente.Telefone = dto.Telefone;
        cliente.WhatsApp = dto.WhatsApp;
        cliente.Email = dto.Email;
        cliente.Cep = dto.Cep;
        cliente.Logradouro = dto.Logradouro;
        cliente.Numero = dto.Numero;
        cliente.Complemento = dto.Complemento;
        cliente.Bairro = dto.Bairro;
        cliente.Cidade = dto.Cidade;
        cliente.Estado = dto.Estado;
        cliente.Observacoes = dto.Observacoes;

        var atualizado = await _clienteRepository.AtualizarAsync(cliente);
        return MapParaDto(atualizado);
    }

    public async Task RemoverAsync(int id)
    {
        if (!await _clienteRepository.ExisteAsync(id))
            throw new DomainException($"Cliente com Id {id} não encontrado.");
        await _clienteRepository.RemoverAsync(id);
    }

    public async Task<int> ContarAsync()
        => await _clienteRepository.ContarAsync();

    private static ClienteDto MapParaDto(Cliente c) => new()
    {
        Id = c.Id, Nome = c.Nome, Telefone = c.Telefone, WhatsApp = c.WhatsApp,
        Email = c.Email, Cep = c.Cep, Logradouro = c.Logradouro, Numero = c.Numero,
        Complemento = c.Complemento, Bairro = c.Bairro, Cidade = c.Cidade,
        Estado = c.Estado, Observacoes = c.Observacoes
    };

    private static Cliente MapParaEntidade(ClienteDto dto) => new()
    {
        Nome = dto.Nome, Telefone = dto.Telefone, WhatsApp = dto.WhatsApp,
        Email = dto.Email, Cep = dto.Cep, Logradouro = dto.Logradouro, Numero = dto.Numero,
        Complemento = dto.Complemento, Bairro = dto.Bairro, Cidade = dto.Cidade,
        Estado = dto.Estado, Observacoes = dto.Observacoes
    };
}
