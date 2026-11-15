using GiaPha.DataAccessObjects.Context;
using GiaPha.Repositories.Interfaces;

namespace GiaPha.Repositories.Implementations;

public sealed class RepositoryManager : IRepositoryManager
{
    private readonly GiaPhaDbContext _context;

    public RepositoryManager(
        GiaPhaDbContext context,
        IClanRepository clanRepository,
        IPersonRepository personRepository,
        IParentChildRepository parentChildRepository,
        IUnionRepository unionRepository,
        IAuditLogRepository auditLogRepository)
    {
        _context = context;
        Clan = clanRepository;
        Person = personRepository;
        ParentChild = parentChildRepository;
        Union = unionRepository;
        AuditLog = auditLogRepository;
    }

    public IClanRepository Clan { get; }

    public IPersonRepository Person { get; }

    public IParentChildRepository ParentChild { get; }

    public IUnionRepository Union { get; }

    public IAuditLogRepository AuditLog { get; }

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
