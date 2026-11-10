using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;

namespace GiaPha.DataAccessObjects.DAOs;

public sealed class ClanDao : GenericDao<Clan>, IClanDao
{
    public ClanDao(GiaPhaDbContext context) : base(context)
    {
    }
}
