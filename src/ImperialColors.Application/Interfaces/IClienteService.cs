using ImperialColors.Application.DTOs;

namespace ImperialColors.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObterTodosAsync();
    Task<ClienteDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<ClienteDto>> BuscarAsync(string nome);
    Task<ClienteDto> CriarAsync(ClienteDto dto);
    Task<ClienteDto> AtualizarAsync(int id, ClienteDto dto);
    Task RemoverAsync(int id);
    Task<int> ContarAsync();
}
