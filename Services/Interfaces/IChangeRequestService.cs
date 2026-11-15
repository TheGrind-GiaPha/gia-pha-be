using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface IChangeRequestService
{
    Task<ChangeRequestDto> CreateAsync(
        Guid clanId,
        Guid requesterId,
        CreateChangeRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChangeRequestDto>> GetByClanAsync(
        Guid clanId,
        ChangeRequestStatus? status,
        Guid? requesterId,
        CancellationToken cancellationToken = default);

    Task<ChangeRequestDto?> GetByIdAsync(
        Guid clanId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ChangeRequestDto> ApproveAsync(
        Guid clanId,
        Guid id,
        Guid approverId,
        CancellationToken cancellationToken = default);

    Task<ChangeRequestDto> RejectAsync(
        Guid clanId,
        Guid id,
        Guid approverId,
        string reason,
        CancellationToken cancellationToken = default);
}
