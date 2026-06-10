using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;

namespace ImperialColors.Infrastructure.Repositories;

public class CategoriaRepository : RepositoryBase<Categoria>, IRepository<Categoria>
{
    public CategoriaRepository(AppDbContext context) : base(context) { }
}
