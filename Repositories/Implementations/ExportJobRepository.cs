using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class ExportJobRepository : RepositoryBase<ExportJob>, IExportJobRepository
{
    public ExportJobRepository(IGenericDao<ExportJob> dao) : base(dao)
    {
    }

    public Task<ExportJob?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
        => FindByCondition(x => x.Id == id, trackChanges)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExportJob>> GetByClanAsync(Guid clanId, bool trackChanges, CancellationToken cancellationToken = default)
        => await FindByCondition(x => x.ClanId == clanId, trackChanges)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
