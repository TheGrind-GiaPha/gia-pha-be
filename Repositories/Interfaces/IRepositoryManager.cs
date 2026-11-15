namespace GiaPha.Repositories.Interfaces;

/// <summary>
/// Repository Manager gom các repository và là nơi duy nhất gọi SaveChanges.
/// Controller/Service không thao tác trực tiếp DbContext.
/// </summary>
public interface IRepositoryManager
{
    IClanRepository Clan { get; }

    IUserRepository User { get; }

    IUserClanRoleRepository UserClanRole { get; }

    IPersonRepository Person { get; }

    IParentChildRepository ParentChild { get; }

    IUnionRepository Union { get; }

    IImportBatchRepository ImportBatch { get; }

    IImportRowRepository ImportRow { get; }

    IExportJobRepository ExportJob { get; }

    IChangeRequestRepository ChangeRequest { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
