using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IImportRowRepository : IRepositoryBase<ImportRow>
{
    Task<ImportRow?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportRow>> GetByBatchAsync(Guid batchId, bool trackChanges, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportRow>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, bool trackChanges, CancellationToken cancellationToken = default);
}
