using GiaPha.DataAccessObjects.Context;
using GiaPha.Repositories.Interfaces;

namespace GiaPha.Repositories.Implementations;

public sealed class RepositoryManager : IRepositoryManager
{
    private readonly GiaPhaDbContext _context;

    public RepositoryManager(
        GiaPhaDbContext context,
        IClanRepository clanRepository,
        IUserRepository userRepository,
        IPersonRepository personRepository,
        IParentChildRepository parentChildRepository,
        IUnionRepository unionRepository)
    {
        _context = context;
        Clan = clanRepository;
        User = userRepository;
        Person = personRepository;
        ParentChild = parentChildRepository;
        Union = unionRepository;
    }

    public IClanRepository Clan { get; }

    public IUserRepository User { get; }

    public IPersonRepository Person { get; }

    public IParentChildRepository ParentChild { get; }

    public IUnionRepository Union { get; }

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
