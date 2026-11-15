using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ImportBatchRepository : RepositoryBase<ImportBatch>, IImportBatchRepository
{
    public ImportBatchRepository(IGenericDao<ImportBatch> dao) : base(dao)
    {
    }

    public Task<ImportBatch?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ImportBatch?> GetWithRowsAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .Include(x => x.Rows.OrderBy(r => r.RowIndex))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ImportBatch>> GetByClanAsync(Guid clanId, bool trackChanges, CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ClanId == clanId, trackChanges)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
