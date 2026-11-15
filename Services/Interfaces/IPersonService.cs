using GiaPha.BusinessObjects.DTOs.Persons;

namespace GiaPha.Services.Interfaces;

public interface IPersonService
{
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
}
