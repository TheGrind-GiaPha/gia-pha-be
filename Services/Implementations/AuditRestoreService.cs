using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Auditing;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class AuditRestoreService : IAuditRestoreService
{
    private readonly IRepositoryManager _repository;
    private readonly IAuditContext _auditContext;

    public AuditRestoreService(IRepositoryManager repository, IAuditContext auditContext)
    {
        _repository = repository;
        _auditContext = auditContext;
    }

    public async Task<RestoreResultDto> RestoreAsync(
        Guid clanId,
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var log = await _repository.AuditLog.GetByIdAsync(logId, trackChanges: false, cancellationToken);
        if (log is null || log.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy bản ghi lịch sử.");
        }

        if (log.Action != AuditAction.Update)
        {
            throw new ConflictException("Chỉ có thể khôi phục bản ghi cập nhật.");
        }

        var before = AuditSnapshot.Parse(log.Before);
        var after = AuditSnapshot.Parse(log.After);

        if (before.Count == 0 || after.Count == 0)
        {
            throw new ConflictException("Bản ghi không có dữ liệu để khôi phục.");
        }

        if (AuditEntityApplier.HasStructuralChange(after))
        {
            throw new ConflictException(
                "Bản ghi chứa thay đổi cấu trúc (xóa, di chuyển, gộp hoặc công bố), chưa thể khôi phục.");
        }

        if (!AuditEntityApplier.HasRestorableField(before))
        {
            throw new ConflictException("Bản ghi không có trường nào có thể khôi phục.");
        }

        var entity = await LoadEntityAsync(clanId, log, cancellationToken);

        var mismatches = AuditEntityApplier.FindMismatches(entity, after);
        if (mismatches.Count > 0)
        {
            throw new ConflictException(
                $"Dữ liệu đã thay đổi ở: {string.Join(", ", mismatches)}. Không thể khôi phục.");
        }

        var applied = AuditEntityApplier.Apply(entity, before);
        if (applied == 0)
        {
            throw new ConflictException("Không có trường nào được khôi phục.");
        }

        if (entity is BaseEntity baseEntity)
        {
            baseEntity.UpdatedAt = DateTime.UtcNow;
        }

        switch (entity)
        {
            case Clan clan:
                _repository.Clan.Update(clan);
                break;
            case Person person:
                _repository.Person.Update(person);
                break;
        }

        _auditContext.IsRestore = true;
        try
        {
            await _repository.SaveAsync(cancellationToken);
        }
        finally
        {
            _auditContext.IsRestore = false;
        }

        return new RestoreResultDto(log.Id, "Đã khôi phục thay đổi.");
    }

    private async Task<object> LoadEntityAsync(
        Guid clanId,
        AuditLog log,
        CancellationToken cancellationToken)
    {
        switch (log.EntityType)
        {
            case "Clan":
            {
                var clan = await _repository.Clan.GetByIdAsync(
                    log.EntityId,
                    trackChanges: true,
                    cancellationToken);

                if (clan is null || clan.Id != clanId)
                {
                    throw new NotFoundException("Không tìm thấy dòng họ.");
                }

                return clan;
            }

            case "Person":
            {
                var person = await _repository.Person.GetByIdAsync(
                    log.EntityId,
                    trackChanges: true,
                    cancellationToken);

                if (person is null || person.ClanId != clanId)
                {
                    throw new NotFoundException("Không tìm thấy thành viên trong dòng họ này.");
                }

                return person;
            }

            default:
                throw new ConflictException($"Chưa hỗ trợ khôi phục {log.EntityType}.");
        }
    }
}
