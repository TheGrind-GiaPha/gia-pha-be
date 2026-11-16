using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;
using GiaPha.Repositories.Interfaces;
namespace GiaPha.Repositories.Implementations;
public sealed class F3Repository(GiaPhaDbContext db) : IF3Repository
{
 public IQueryable<Person> People => db.Persons;
 public IQueryable<Clan> Clans => db.Clans;
 public IQueryable<Branch> Branches => db.Branches;
 public IQueryable<UserClanRole> Roles => db.UserClanRoles;
 public IQueryable<ParentChild> Parents => db.ParentChild;
 public IQueryable<Union> Unions => db.Unions;
 public IQueryable<PersonProfileExtension> Profiles => db.Set<PersonProfileExtension>();
 public IQueryable<PersonPhoto> Photos => db.Set<PersonPhoto>();
 public void Add<T>(T entity) where T:class => db.Add(entity);
 public void Remove<T>(T entity) where T:class => db.Remove(entity);
 public async Task SaveAsync(CancellationToken ct=default) => await db.SaveChangesAsync(ct);
}
