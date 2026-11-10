using GiaPha.BusinessObjects.Entities;

namespace GiaPha.DataAccessObjects.DAOs;

/// <summary>
/// DAO gốc: chỉ làm việc với DbContext, không chứa business logic.
/// Repository sẽ gọi DAO, controller/service không được gọi DbContext trực tiếp.
/// </summary>
public interface IGenericDao<T> where T : BaseEntity
{
    IQueryable<T> Query(bool trackChanges);

    Task<T?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default);

    void Add(T entity);

    void Update(T entity);

    void Remove(T entity);
}
