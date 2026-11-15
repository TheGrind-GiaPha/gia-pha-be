using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IExportJobRepository : IRepositoryBase<ExportJob>
{
    Task<ExportJob?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExportJob>> GetByClanAsync(Guid clanId, bool trackChanges, CancellationToken cancellationToken = default);
}
