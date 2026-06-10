using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;

namespace ImperialColors.Infrastructure.Repositories;

public class MarcaRepository : RepositoryBase<Marca>, IRepository<Marca>
{
    public MarcaRepository(AppDbContext context) : base(context) { }
}
