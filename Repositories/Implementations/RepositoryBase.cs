using System.Linq.Expressions;
using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;

namespace GiaPha.Repositories.Implementations;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : BaseEntity
{
    private readonly IGenericDao<T> _dao;

    protected RepositoryBase(IGenericDao<T> dao)
    {
        _dao = dao;
    }

    protected IGenericDao<T> Dao => _dao;

    public IQueryable<T> FindAll(bool trackChanges)
        => _dao.Query(trackChanges);

    public IQueryable<T> FindByCondition(
        Expression<Func<T, bool>> expression,
        bool trackChanges)
        => _dao.Query(trackChanges).Where(expression);

    public void Create(T entity) => _dao.Add(entity);

    public void Update(T entity) => _dao.Update(entity);

    public void Delete(T entity) => _dao.Remove(entity);
}
