using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Implementations;

public sealed class ChangeRequestService : IChangeRequestService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IClanRoleService _roleService;
    private readonly IReadOnlyDictionary<string, IChangeHandler> _handlers;

    public ChangeRequestService(
        IRepositoryManager repository,
        ILoggerManager logger,
        IClanRoleService roleService,
        IEnumerable<IChangeHandler> handlers)
    {
        _repository = repository;
        _logger = logger;
        _roleService = roleService;
        _handlers = handlers.ToDictionary(h => h.Type);
    }

    public async Task<ChangeRequestDto> CreateAsync(
        Guid clanId,
        Guid requesterId,
        CreateChangeRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(request.Type, out var handler))
        {
            throw new ArgumentException($"Loại thay đổi '{request.Type}' không hợp lệ.", nameof(request));
        }

        var payloadJson = request.Payload is { } payload ? payload.GetRawText() : "{}";
        var summary = await handler.SummarizeAsync(payloadJson, cancellationToken);

        var changeRequest = new ChangeRequest
        {
            ClanId = clanId,
            RequesterId = requesterId,
            Type = request.Type,
            PayloadJson = payloadJson,
            Summary = summary,
            Status = ChangeRequestStatus.Pending,
            DueAt = DateTime.UtcNow.AddDays(3)
        };

        _repository.ChangeRequest.Create(changeRequest);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã tạo đề xuất {request.Type} cho clan {clanId}.");
        return Map(changeRequest);
    }

    public async Task<IReadOnlyList<ChangeRequestDto>> GetByClanAsync(
        Guid clanId,
        ChangeRequestStatus? status,
        Guid? requesterId,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.ChangeRequest.GetByClanAsync(
            clanId, status, requesterId, trackChanges: false, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<ChangeRequestDto?> GetByIdAsync(
        Guid clanId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.ChangeRequest.GetByIdAsync(id, trackChanges: false, cancellationToken);
        return item is null || item.ClanId != clanId ? null : Map(item);
    }

    public async Task<ChangeRequestDto> ApproveAsync(
        Guid clanId,
        Guid id,
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        var changeRequest = await LoadForDecisionAsync(clanId, id, cancellationToken);

        if (changeRequest.RequesterId == approverId)
        {
            throw new ForbiddenException("Không được tự duyệt đề xuất của chính mình (SELF_APPROVAL).");
        }

        await EnsureCanApproveAsync(approverId, clanId, cancellationToken);

        if (!_handlers.TryGetValue(changeRequest.Type, out var handler))
        {
            throw new ArgumentException($"Loại thay đổi '{changeRequest.Type}' không hợp lệ.");
        }

        // Apply + AuditLog trong cùng một transaction.
        await handler.ApplyAsync(changeRequest.PayloadJson, cancellationToken);

        _repository.AuditLog.Create(new AuditLog
        {
            ClanId = clanId,
            EntityType = changeRequest.Type,
            ChangeRequestId = changeRequest.Id,
            ActorId = approverId,
            Action = AuditAction.Update,
            AfterJson = changeRequest.PayloadJson,
            OccurredAt = DateTime.UtcNow
        });

        changeRequest.Status = ChangeRequestStatus.Approved;
        changeRequest.ReviewerId = approverId;
        changeRequest.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã duyệt đề xuất {id} bởi {approverId}.");
        return Map(changeRequest);
    }

    public async Task<ChangeRequestDto> RejectAsync(
        Guid clanId,
        Guid id,
        Guid approverId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var changeRequest = await LoadForDecisionAsync(clanId, id, cancellationToken);

        if (changeRequest.RequesterId == approverId)
        {
            throw new ForbiddenException("Không được tự duyệt đề xuất của chính mình (SELF_APPROVAL).");
        }

        await EnsureCanApproveAsync(approverId, clanId, cancellationToken);

        changeRequest.Status = ChangeRequestStatus.Rejected;
        changeRequest.RejectReason = reason.Trim();
        changeRequest.ReviewerId = approverId;
        changeRequest.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã từ chối đề xuất {id} bởi {approverId}.");
        return Map(changeRequest);
    }

    private async Task<ChangeRequest> LoadForDecisionAsync(
        Guid clanId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var changeRequest = await _repository.ChangeRequest.GetByIdAsync(id, trackChanges: true, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy đề xuất.");

        if (changeRequest.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy đề xuất.");
        }

        if (changeRequest.Status != ChangeRequestStatus.Pending)
        {
            throw new ConflictException("Đề xuất đã được xử lý.");
        }

        return changeRequest;
    }

    private async Task EnsureCanApproveAsync(Guid userId, Guid clanId, CancellationToken cancellationToken)
    {
        var isApprover = await _roleService.HasRoleAsync(userId, clanId, ClanRole.Approver, cancellationToken);
        var isAdmin = await _roleService.HasRoleAsync(userId, clanId, ClanRole.ClanAdmin, cancellationToken);

        if (!isApprover && !isAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền duyệt đề xuất trong dòng họ này.");
        }
    }

    private static ChangeRequestDto Map(ChangeRequest cr)
        => new(cr.Id, cr.ClanId, cr.Type, cr.Summary, cr.Status, cr.RequesterId, cr.CreatedAt, cr.DueAt, cr.RejectReason);
}
