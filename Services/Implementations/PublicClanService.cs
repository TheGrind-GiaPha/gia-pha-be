using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.DTOs.Public;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class PublicClanService : IPublicClanService
{
    private readonly IRepositoryManager _repository;

    public PublicClanService(IRepositoryManager repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PublicClanDto>> GetPublicClansAsync(
        CancellationToken cancellationToken = default)
    {
        var clans = await _repository.Clan.GetPublicAsync(cancellationToken);
        return clans
            .Select(c => new PublicClanDto(c.Id, c.Name, c.Description))
            .ToList();
    }

    public async Task<PublicPersonNodeDto?> GetPublicTreeAsync(
        Guid clanId,
        int depth = 20,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: false, cancellationToken);
        if (clan is null || clan.Visibility != ClanVisibility.Public)
        {
            // Không lộ sự tồn tại của dòng họ riêng tư.
            return null;
        }

        if (depth < 0)
        {
            depth = 0;
        }

        var persons = await _repository.Person.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        if (persons.Count == 0)
        {
            return null;
        }

        var personIds = persons.Select(p => p.Id).ToList();
        var byId = persons.ToDictionary(p => p.Id);

        var links = await _repository.ParentChild
            .GetByParentIdsAsync(personIds, RelationType.Biological, cancellationToken);

        var childrenMap = links
            .GroupBy(l => l.ParentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => l.SiblingOrder).Select(l => l.ChildId).ToList());

        var childIds = links.Select(l => l.ChildId).ToHashSet();
        var root = persons
            .Where(p => !childIds.Contains(p.Id))
            .OrderBy(p => p.Generation)
            .FirstOrDefault();

        return root is null ? null : BuildNode(root, childrenMap, byId, depth);
    }

    private static PublicPersonNodeDto BuildNode(
        Person person,
        IReadOnlyDictionary<Guid, List<Guid>> childrenMap,
        IReadOnlyDictionary<Guid, Person> byId,
        int depth)
    {
        var children = new List<PublicPersonNodeDto>();
        if (depth > 0 && childrenMap.TryGetValue(person.Id, out var childIds))
        {
            foreach (var childId in childIds)
            {
                if (byId.TryGetValue(childId, out var child))
                {
                    children.Add(BuildNode(child, childrenMap, byId, depth - 1));
                }
            }
        }

        return new PublicPersonNodeDto(Map(person), children);
    }

    private static PublicPersonDto Map(Person person)
    {
        // Privacy: người còn sống chỉ lộ tên/giới tính/đời; ẩn ngày sinh-mất và ảnh.
        var hidePrivate = person.IsLiving;

        return new PublicPersonDto(
            person.Id,
            person.FullName,
            person.Gender,
            person.Generation,
            person.IsLiving,
            hidePrivate ? null : Map(person.BirthDate),
            hidePrivate ? null : Map(person.DeathDate),
            hidePrivate ? null : person.PortraitUrl);
    }

    private static FlexibleDateDto? Map(FlexibleDate? date)
        => date is null
            ? null
            : new FlexibleDateDto(date.Year, date.Month, date.Day, date.Calendar, date.IsLeapMonth);
}
