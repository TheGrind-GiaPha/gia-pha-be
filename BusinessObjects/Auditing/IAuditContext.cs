using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiaPha.BusinessObjects.Auditing;

public interface IAuditContext
{
    Guid BatchId { get; }
    Guid? ActorId { get; set; }
    Guid? ChangeRequestId { get; set; }
}
