using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.DAOs;

public class GenericDao<T> : IGenericDao<T> where T : BaseEntity
{
    private readonly GiaPhaDbContext _context;

    public GenericDao(GiaPhaDbContext context)
    {
        _context = context;
    }

    protected GiaPhaDbContext Context => _context;

    protected DbSet<T> Set => _context.Set<T>();

    public IQueryable<T> Query(bool trackChanges)
        => trackChanges ? Set : Set.AsNoTracking();

    public Task<T?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
        => Query(trackChanges).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(T entity) => Set.Add(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);
}
