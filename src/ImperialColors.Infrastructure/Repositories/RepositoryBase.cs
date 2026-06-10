using ImperialColors.Domain.Entities;
using ImperialColors.Domain.Interfaces;
using ImperialColors.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ImperialColors.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(int id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.Ativo);

    public virtual async Task<IEnumerable<T>> ObterTodosAsync()
        => await _dbSet.Where(e => e.Ativo).ToListAsync();

    public virtual async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> AdicionarAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> AtualizarAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task RemoverAsync(int id)
    {
        var entity = await ObterPorIdAsync(id);
        if (entity is not null)
        {
            entity.Ativo = false;
            entity.AtualizadoEm = DateTime.UtcNow;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExisteAsync(int id)
        => await _dbSet.AnyAsync(e => e.Id == id && e.Ativo);

    public virtual async Task<int> ContarAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate is null)
            return await _dbSet.CountAsync(e => e.Ativo);
        return await _dbSet.CountAsync(predicate);
    }
}
