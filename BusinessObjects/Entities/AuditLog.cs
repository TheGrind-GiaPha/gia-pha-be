using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiaPha.BusinessObjects.Entities;

public class AuditLog : BaseEntity
{
    public Guid ClanId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public AuditAction Action { get; set; }
    public Guid? ActorId { get; set; }
    public Guid? ChangeRequestId { get; set; }
    public Guid BatchId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

}
