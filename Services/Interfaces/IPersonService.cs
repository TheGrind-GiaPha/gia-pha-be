using GiaPha.BusinessObjects.DTOs.Persons;

namespace GiaPha.Services.Interfaces;

public interface IPersonService
{
    Task<PersonDto?> GetByIdAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default);

    Task<PersonProfileDto?> GetProfileAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default);

    Task<PersonDto> CreateRootAsync(
        Guid clanId,
        PersonProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<PersonProfileDto?> UpdateAsync(
        Guid clanId,
        Guid personId,
        PersonProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonDto>> AddChildrenAsync(
        Guid clanId,
        Guid fatherId,
        AddChildrenRequest request,
        CancellationToken cancellationToken = default);

    Task<UnionDto> AddUnionAsync(
        Guid clanId,
        Guid husbandId,
        AddUnionRequest request,
        CancellationToken cancellationToken = default);

    Task MoveSubtreeAsync(
        Guid clanId,
        Guid personId,
        Guid newFatherId,
        CancellationToken cancellationToken = default);

    Task DeletePersonAsync(
        Guid clanId,
        Guid personId,
        string strategy,
        Guid? newFatherId = null,
        CancellationToken cancellationToken = default);

    Task<PersonNodeDto?> GetTreeAsync(
        Guid clanId,
        Guid? rootId = null,
        int depth = 3,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonDto>> GetAncestorsAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonDto>> GetDescendantsAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default);

    Task<PersonDto> MergePersonsAsync(
        Guid clanId,
        Guid keepId,
        Guid removeId,
        CancellationToken cancellationToken = default);

    Task<GraftResultDto> GraftAsync(
        Guid clanId,
        Guid rootId,
        Guid newFatherId,
        CancellationToken cancellationToken = default);

    Task SetPortraitAsync(
        Guid clanId,
        Guid personId,
        string portraitUrl,
        CancellationToken cancellationToken = default);
}
