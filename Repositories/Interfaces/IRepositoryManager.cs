namespace GiaPha.Repositories.Interfaces;

/// <summary>
/// Repository Manager gom các repository và là nơi duy nhất gọi SaveChanges.
/// Controller/Service không thao tác trực tiếp DbContext.
/// </summary>
public interface IRepositoryManager
{
    IClanRepository Clan { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
