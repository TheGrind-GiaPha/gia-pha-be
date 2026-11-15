using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiaPha.BusinessObjects.Entities;

public interface IClanScopedEntity
{
    Guid ClanId { get; }
}
