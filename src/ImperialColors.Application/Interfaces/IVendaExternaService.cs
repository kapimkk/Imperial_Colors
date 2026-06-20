using ImperialColors.Application.DTOs;

namespace ImperialColors.Application.Interfaces;

public interface IVendaExternaService
{
    Task<IEnumerable<VendaExternaDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<VendaExternaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<VendaExternaDto> RegistrarAsync(RegistrarVendaExternaDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LinhaImportacaoVendaExternaDto>> ProcessarImportacaoTxtAsync(string conteudoArquivo, CancellationToken cancellationToken = default);
}
