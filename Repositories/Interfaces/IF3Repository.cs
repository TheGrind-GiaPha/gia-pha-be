using GiaPha.BusinessObjects.Entities;
namespace GiaPha.Repositories.Interfaces;
public interface IF3Repository
{
 IQueryable<Person> People { get; }
 IQueryable<Clan> Clans { get; }
 IQueryable<Branch> Branches { get; }
 IQueryable<UserClanRole> Roles { get; }
 IQueryable<ParentChild> Parents { get; }
 IQueryable<Union> Unions { get; }
 IQueryable<PersonProfileExtension> Profiles { get; }
 IQueryable<PersonPhoto> Photos { get; }
 void Add<T>(T entity) where T: class;
 void Remove<T>(T entity) where T: class;
 Task SaveAsync(CancellationToken ct=default);
}
