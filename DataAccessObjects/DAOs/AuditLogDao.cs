using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;

namespace GiaPha.DataAccessObjects.DAOs;

public sealed class AuditLogDao : GenericDao<AuditLog>, IAuditLogDao
{
    public AuditLogDao(GiaPhaDbContext context) : base(context)
    {
    }
}
