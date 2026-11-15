using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ImportRowRepository : RepositoryBase<ImportRow>, IImportRowRepository
{
    public ImportRowRepository(IGenericDao<ImportRow> dao) : base(dao)
    {
    }

    public Task<ImportRow?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ImportRow>> GetByBatchAsync(Guid batchId, bool trackChanges, CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.BatchId == batchId, trackChanges)
            .OrderBy(x => x.RowIndex)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ImportRow>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, bool trackChanges, CancellationToken cancellationToken = default)
        => await FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .OrderBy(x => x.RowIndex)
            .ToListAsync(cancellationToken);
}
