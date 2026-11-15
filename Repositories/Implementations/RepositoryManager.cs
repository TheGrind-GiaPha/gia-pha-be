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
        IUserClanRoleRepository userClanRoleRepository,
        IPersonRepository personRepository,
        IParentChildRepository parentChildRepository,
        IUnionRepository unionRepository,
        IImportBatchRepository importBatchRepository,
        IImportRowRepository importRowRepository,
        IExportJobRepository exportJobRepository,
        IChangeRequestRepository changeRequestRepository)
    {
        _context = context;
        Clan = clanRepository;
        User = userRepository;
        UserClanRole = userClanRoleRepository;
        Person = personRepository;
        ParentChild = parentChildRepository;
        Union = unionRepository;
        ImportBatch = importBatchRepository;
        ImportRow = importRowRepository;
        ExportJob = exportJobRepository;
        ChangeRequest = changeRequestRepository;
    }

    public IClanRepository Clan { get; }

    public IUserRepository User { get; }

    public IUserClanRoleRepository UserClanRole { get; }

    public IPersonRepository Person { get; }

    public IParentChildRepository ParentChild { get; }

    public IUnionRepository Union { get; }

    public IImportBatchRepository ImportBatch { get; }

    public IImportRowRepository ImportRow { get; }

    public IExportJobRepository ExportJob { get; }

    public IChangeRequestRepository ChangeRequest { get; }

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
