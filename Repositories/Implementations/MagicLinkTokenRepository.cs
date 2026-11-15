using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.Repositories.Implementations;

public sealed class MagicLinkTokenRepository : RepositoryBase<MagicLinkToken>, IMagicLinkTokenRepository
{
    public MagicLinkTokenRepository(IGenericDao<MagicLinkToken> dao) : base(dao)
    {
    }

    public Task<MagicLinkToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
        => FindByCondition(x => x.TokenHash == tokenHash, trackChanges: true)
            .FirstOrDefaultAsync(cancellationToken);
}
