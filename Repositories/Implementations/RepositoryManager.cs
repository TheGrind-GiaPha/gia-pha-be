using GiaPha.DataAccessObjects.Context;
using GiaPha.Repositories.Interfaces;

namespace GiaPha.Repositories.Implementations;

public sealed class RepositoryManager : IRepositoryManager
{
    private readonly GiaPhaDbContext _context;

    public RepositoryManager(
        GiaPhaDbContext context,
        IClanRepository clanRepository)
    {
        _context = context;
        Clan = clanRepository;
    }

    public IClanRepository Clan { get; }

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
