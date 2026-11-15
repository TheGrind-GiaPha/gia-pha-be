using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IImportBatchRepository : IRepositoryBase<ImportBatch>
{
    Task<ImportBatch?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default);

    Task<ImportBatch?> GetWithRowsAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportBatch>> GetByClanAsync(Guid clanId, bool trackChanges, CancellationToken cancellationToken = default);
}
