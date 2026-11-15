using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiaPha.BusinessObjects.Auditing;

public sealed class AuditContext : IAuditContext
{
    public Guid BatchId { get; } = Guid.NewGuid();
    public Guid? ActorId { get; set; }
    public Guid? ChangeRequestId { get; set; }
}
