using System.Linq.Expressions;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

/// <summary>
/// Interface gốc giống mẫu trong slide PRN232:
/// FindAll, FindByCondition, Create, Update, Delete.
/// </summary>
public interface IRepositoryBase<T> where T : BaseEntity
{
    IQueryable<T> FindAll(bool trackChanges);

    IQueryable<T> FindByCondition(
        Expression<Func<T, bool>> expression,
        bool trackChanges);

    void Create(T entity);

    void Update(T entity);

    void Delete(T entity);
}
