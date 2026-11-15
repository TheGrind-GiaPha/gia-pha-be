using System.Text.Json;
using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GiaPha.DataAccessObjects.Auditing;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.Ordinal)
    {
        nameof(BaseEntity.Id),
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.UpdatedAt)
    };

    private readonly IAuditContext _auditContext;
    public AuditSaveChangesInterceptor(IAuditContext auditContext)
    {
        _auditContext = auditContext;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return;
        }
        var entries = context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Where(entry => entry.Entity is IClanScopedEntity)
            .ToList();

        foreach (var entry in entries)
        {
            var clanScoped = (IClanScopedEntity)entry.Entity;
            var (before, after) = BuildSnapshots(entry);

            if (entry.State == EntityState.Modified && before is null)
            {
                continue;
            }

            context.Add(new AuditLog
            {
                ClanId = clanScoped.ClanId,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Property("Id").CurrentValue is Guid id ? id : Guid.Empty,
                Action = entry.State == EntityState.Added
                    ? AuditAction.Create
                    : _auditContext.IsRestore
                        ? AuditAction.Restore
                        : AuditAction.Update,
                BatchId = _auditContext.BatchId,
                Before = before,
                After = after,
                OccurredAt = DateTime.UtcNow
            });
        }
    }

    private static (string? Before, string? After) BuildSnapshots(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
        {
            return (null, JsonSerializer.Serialize(entry.Entity, entry.Entity.GetType(), AuditJson.Options));
        }

        var before = new Dictionary<string, object?>();
        var after = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified || IgnoredProperties.Contains(property.Metadata.Name))
            {
                continue;
            }

            if (Equals(property.OriginalValue, property.CurrentValue))
            {
                continue;
            }

            var key = JsonNamingPolicy.CamelCase.ConvertName(property.Metadata.Name);
            before[key] = property.OriginalValue;
            after[key] = property.CurrentValue;
        }

        return before.Count == 0
            ? (null, null)
            : (JsonSerializer.Serialize(before, AuditJson.Options), JsonSerializer.Serialize(after, AuditJson.Options));
    }
}
