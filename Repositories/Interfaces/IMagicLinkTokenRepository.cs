using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Repositories.Interfaces;

public interface IMagicLinkTokenRepository : IRepositoryBase<MagicLinkToken>
{
    Task<MagicLinkToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
