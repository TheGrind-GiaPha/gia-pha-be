using System.Globalization;
using System.Text;
using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Implementations;

public sealed class PersonService : IPersonService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public PersonService(IRepositoryManager repository, ILoggerManager logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PersonDto>> AddChildrenAsync(
        Guid clanId,
        Guid fatherId,
        AddChildrenRequest request,
        CancellationToken cancellationToken = default)
    {
        var father = await _repository.Person.GetByIdAsync(fatherId, trackChanges: false, cancellationToken);
        if (father is null || father.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người cha trong dòng họ này.");
        }

        var startOrder = await _repository.ParentChild.GetMaxSiblingOrderAsync(fatherId, cancellationToken);

        var children = new List<PersonDto>(request.Children.Count);
        for (var i = 0; i < request.Children.Count; i++)
        {
            var item = request.Children[i];

            var child = new Person
            {
                ClanId = clanId,
                FullName = NormalizeName(item.FullName),
                Gender = item.Gender,
                BirthDate = Map(item.BirthDate),
                IsLiving = true,
                Generation = father.Generation + 1,
                Status = PersonStatus.Active,
                PrivacyLevel = PrivacyLevel.Public,
                Version = 1
            };

            // Path = Path của cha + id của con + "/" (materialized path).
            child.Path = father.Path + child.Id + "/";

            _repository.Person.Create(child);
            _repository.ParentChild.Create(new ParentChild
            {
                ParentId = fatherId,
                ChildId = child.Id,
                RelationType = item.RelationType,
                SiblingOrder = startOrder + i + 1
            });

            children.Add(Map(child));
        }

        // Một lần SaveChanges duy nhất => tất cả con + quan hệ cha-con trong cùng 1 transaction.
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã thêm {children.Count} người con cho cha {fatherId}.");
        return children;
    }

    public async Task<UnionDto> AddUnionAsync(
        Guid clanId,
        Guid husbandId,
        AddUnionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.WifeId == Guid.Empty)
        {
            throw new ArgumentException("Người vợ là bắt buộc.", nameof(request));
        }

        var husband = await _repository.Person.GetByIdAsync(husbandId, trackChanges: false, cancellationToken);
        if (husband is null || husband.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người chồng trong dòng họ này.");
        }

        var wife = await _repository.Person.GetByIdAsync(request.WifeId, trackChanges: false, cancellationToken);
        if (wife is null || wife.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người vợ trong dòng họ này.");
        }

        var rank = request.Rank <= 0 ? 1 : request.Rank;
        if (await _repository.Union.ExistsByHusbandAndRankAsync(husbandId, rank, cancellationToken))
        {
            throw new ConflictException($"Người chồng đã có vợ với Rank {rank}.");
        }

        var union = new Union
        {
            HusbandId = husbandId,
            WifeId = request.WifeId,
            Rank = rank
        };

        _repository.Union.Create(union);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã thêm quan hệ vợ chồng {husbandId} - {request.WifeId} (Rank {rank}).");
        return Map(union);
    }

    public async Task MoveSubtreeAsync(
        Guid clanId,
        Guid personId,
        Guid newFatherId,
        CancellationToken cancellationToken = default)
    {
        if (newFatherId == Guid.Empty)
        {
            throw new ArgumentException("Người cha mới là bắt buộc.", nameof(newFatherId));
        }

        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: true, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người cần di chuyển.");
        }

        var newFather = await _repository.Person.GetByIdAsync(newFatherId, trackChanges: false, cancellationToken);
        if (newFather is null || newFather.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người cha mới.");
        }

        if (personId == newFatherId)
        {
            throw new ConflictException("Không thể gán cha là chính mình.");
        }

        EnsureNoCycle(person.Path, newFather.Path);

        var link = await _repository.ParentChild.GetByChildIdAsync(personId, RelationType.Biological, cancellationToken);
        if (link is null)
        {
            throw new ConflictException("Người này không có cha (nút gốc) nên không thể di chuyển.");
        }

        var oldPrefix = person.Path;
        var newPrefix = newFather.Path + person.Id + "/";
        var delta = newFather.Generation + 1 - person.Generation;

        // Đổi tiền tố Path + cộng delta Generation cho toàn bộ cây con.
        var subtree = await _repository.Person.GetByPathPrefixAsync(oldPrefix, trackChanges: true, cancellationToken);
        foreach (var p in subtree)
        {
            p.Path = newPrefix + p.Path.Substring(oldPrefix.Length);
            p.Generation += delta;
        }

        // Chuyển quan hệ cha sang người cha mới.
        link.ParentId = newFatherId;
        link.SiblingOrder = await _repository.ParentChild.GetMaxSiblingOrderAsync(newFatherId, cancellationToken) + 1;

        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã di chuyển cây con {personId} sang cha {newFatherId}.");
    }

    public async Task DeletePersonAsync(
        Guid clanId,
        Guid personId,
        string strategy,
        Guid? newFatherId = null,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: true, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người cần xóa.");
        }

        switch (strategy?.ToLowerInvariant())
        {
            case "block":
                if (await _repository.ParentChild.ExistsByParentAsync(personId, cancellationToken))
                {
                    throw new ConflictException("Người này vẫn còn con, không thể xóa (HAS_DESCENDANTS).");
                }

                person.Status = PersonStatus.Deleted;
                break;

            case "cascade":
                var subtree = await _repository.Person.GetByPathPrefixAsync(person.Path, trackChanges: true, cancellationToken);
                foreach (var p in subtree)
                {
                    p.Status = PersonStatus.Deleted;
                }

                break;

            case "reparent":
                if (newFatherId is null || newFatherId == Guid.Empty)
                {
                    throw new ArgumentException("Chiến lược reparent cần newFatherId.", nameof(newFatherId));
                }

                var childLinks = await _repository.ParentChild
                    .GetByParentIdsAsync(new[] { personId }, RelationType.Biological, cancellationToken);

                foreach (var childLink in childLinks)
                {
                    await MoveSubtreeAsync(clanId, childLink.ChildId, newFatherId.Value, cancellationToken);
                }

                person.Status = PersonStatus.Deleted;
                break;

            default:
                throw new ArgumentException($"Chiến lược xóa không hợp lệ: {strategy}.", nameof(strategy));
        }

        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã xóa (soft-delete) người {personId} với chiến lược {strategy}.");
    }

    public async Task<PersonNodeDto?> GetTreeAsync(
        Guid clanId,
        Guid? rootId = null,
        int depth = 3,
        CancellationToken cancellationToken = default)
    {
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

        var links = await _repository.ParentChild.GetByParentIdsAsync(personIds, RelationType.Biological, cancellationToken);
        var childrenMap = links
            .GroupBy(l => l.ParentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => l.SiblingOrder).Select(l => l.ChildId).ToList());

        Person? root;
        if (rootId is not null)
        {
            root = byId.GetValueOrDefault(rootId.Value);
        }
        else
        {
            var childIds = links.Select(l => l.ChildId).ToHashSet();
            root = persons
                .Where(p => !childIds.Contains(p.Id))
                .OrderBy(p => p.Generation)
                .FirstOrDefault();
        }

        return root is null ? null : BuildNode(root, childrenMap, byId, depth);
    }

    public async Task<IReadOnlyList<PersonDto>> GetAncestorsAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: false, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người.");
        }

        var ancestorIds = GetAncestorIds(person.Path);
        if (ancestorIds.Count == 0)
        {
            return Array.Empty<PersonDto>();
        }

        var ancestors = await _repository.Person.GetByIdsAsync(ancestorIds, trackChanges: false, cancellationToken);
        var byId = ancestors.ToDictionary(p => p.Id);

        // Trả theo thứ tự từ tổ tiên xa nhất đến gần nhất (theo thứ tự trong Path).
        return ancestorIds.Where(byId.ContainsKey).Select(id => Map(byId[id])).ToList();
    }

    public async Task<IReadOnlyList<PersonDto>> GetDescendantsAsync(
        Guid clanId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: false, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người.");
        }

        var subtree = await _repository.Person.GetByPathPrefixAsync(person.Path, trackChanges: false, cancellationToken);
        return subtree
            .Where(p => p.Id != personId)
            .OrderBy(p => p.Generation)
            .ThenBy(p => p.FullName)
            .Select(Map)
            .ToList();
    }

    public async Task<PersonDto> MergePersonsAsync(
        Guid clanId,
        Guid keepId,
        Guid removeId,
        CancellationToken cancellationToken = default)
    {
        var keep = await _repository.Person.GetByIdAsync(keepId, trackChanges: true, cancellationToken);
        if (keep is null || keep.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người giữ lại (keep).");
        }

        var remove = await _repository.Person.GetByIdAsync(removeId, trackChanges: true, cancellationToken);
        if (remove is null || remove.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người bị gộp (remove).");
        }

        if (keepId == removeId)
        {
            throw new ConflictException("Không thể gộp một người với chính mình.");
        }

        if (keep.Path.StartsWith(remove.Path, StringComparison.Ordinal)
            || remove.Path.StartsWith(keep.Path, StringComparison.Ordinal))
        {
            throw new ConflictException("Hai người nằm trên cùng dòng tổ tiên, không thể gộp.");
        }

        // Chuyển con ruột của remove sang keep (re-path + đổi generation).
        var childLinks = await _repository.ParentChild
            .GetByParentIdsAsync(new[] { removeId }, RelationType.Biological, cancellationToken);

        foreach (var childLink in childLinks)
        {
            await MoveSubtreeAsync(clanId, childLink.ChildId, keepId, cancellationToken);
        }

        // Xóa quan hệ "là con" của remove (cha ruột / cha nuôi).
        var removeParentLinks = await _repository.ParentChild.GetAllByChildIdAsync(removeId, cancellationToken);
        foreach (var link in removeParentLinks)
        {
            _repository.ParentChild.Delete(link);
        }

        // Chuyển quan hệ vợ/chồng của remove sang keep.
        var unions = await _repository.Union.GetByPersonIdAsync(removeId, cancellationToken);
        foreach (var union in unions)
        {
            if (union.HusbandId == removeId)
            {
                union.HusbandId = keepId;
            }

            if (union.WifeId == removeId)
            {
                union.WifeId = keepId;
            }
        }

        // Điền field trống của keep từ remove (field đã có của keep được giữ nguyên).
        keep.HuyName ??= remove.HuyName;
        keep.TuName ??= remove.TuName;
        keep.ThuyName ??= remove.ThuyName;
        keep.BirthDate ??= remove.BirthDate;
        keep.DeathDate ??= remove.DeathDate;
        keep.PortraitUrl ??= remove.PortraitUrl;
        keep.BranchId ??= remove.BranchId;

        // Soft-delete remove + lưu MergedIntoId.
        remove.Status = PersonStatus.Deleted;
        remove.MergedIntoId = keepId;
        remove.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã gộp người {removeId} vào {keepId}.");

        return Map(keep);
    }

    public async Task<GraftResultDto> GraftAsync(
        Guid clanId,
        Guid rootId,
        Guid newFatherId,
        CancellationToken cancellationToken = default)
    {
        await MoveSubtreeAsync(clanId, rootId, newFatherId, cancellationToken);

        var root = await _repository.Person.GetByIdAsync(rootId, trackChanges: false, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người vừa ghép.");

        // Phát hiện trùng: cùng tên (bỏ dấu) + cùng năm sinh.
        var persons = await _repository.Person.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        var normalizedName = StripDiacritics(root.FullName);
        var birthYear = root.BirthDate?.Year;

        var candidates = persons
            .Where(p => p.Id != rootId
                && p.BirthDate?.Year == birthYear
                && StripDiacritics(p.FullName) == normalizedName)
            .Select(Map)
            .ToList();

        return new GraftResultDto(Map(root), candidates);
    }

    public async Task SetPortraitAsync(
        Guid clanId,
        Guid personId,
        string portraitUrl,
        CancellationToken cancellationToken = default)
    {
        var person = await _repository.Person.GetByIdAsync(personId, trackChanges: true, cancellationToken);
        if (person is null || person.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy người.");
        }

        person.PortraitUrl = portraitUrl;
        person.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã cập nhật chân dung cho người {personId}.");
    }

    /// <summary>
    /// Chống vòng lặp: nếu Path của cha mới nằm "bên dưới" Path của người đang xét
    /// (cha mới là con/cháu của chính người đó) thì gán cha sẽ tạo vòng lặp.
    /// </summary>
    public static void EnsureNoCycle(string personPath, string newFatherPath)
    {
        if (!string.IsNullOrEmpty(newFatherPath)
            && newFatherPath.StartsWith(personPath, StringComparison.Ordinal))
        {
            throw new ConflictException("Không thể gán cha vì sẽ tạo vòng lặp trong gia phả (CYCLE_DETECTED).");
        }
    }

    private static PersonNodeDto BuildNode(
        Person person,
        IReadOnlyDictionary<Guid, List<Guid>> childrenMap,
        IReadOnlyDictionary<Guid, Person> byId,
        int depth)
    {
        var children = new List<PersonNodeDto>();
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

        return new PersonNodeDto(Map(person), children);
    }

    private static IReadOnlyList<Guid> GetAncestorIds(string path)
    {
        // Path = "/id1/id2/id3/" -> tổ tiên là [id1, id2], bỏ id3 (bản thân).
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Take(segments.Length - 1).Select(Guid.Parse).ToList();
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Họ tên người không được để trống.", nameof(name));
        }

        return normalized;
    }

    /// <summary>Bỏ dấu + lowercase tên để so sánh trùng (ví dụ "Nguyễn Văn" == "nguyen van").</summary>
    private static string StripDiacritics(string name)
    {
        var decomposed = name.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    private static FlexibleDate? Map(FlexibleDateDto? dto)
        => dto is null
            ? null
            : new FlexibleDate
            {
                Year = dto.Year,
                Month = dto.Month,
                Day = dto.Day,
                Calendar = dto.Calendar,
                IsLeapMonth = dto.IsLeapMonth
            };

    private static FlexibleDateDto? Map(FlexibleDate? date)
        => date is null
            ? null
            : new FlexibleDateDto(date.Year, date.Month, date.Day, date.Calendar, date.IsLeapMonth);

    private static PersonDto Map(Person person)
        => new(
            person.Id,
            person.ClanId,
            person.FullName,
            person.HuyName,
            person.Gender,
            Map(person.BirthDate),
            Map(person.DeathDate),
            person.Generation,
            person.Path,
            person.PortraitUrl,
            person.HasHiddenChildren);

    private static UnionDto Map(Union union)
        => new(union.Id, union.HusbandId, union.WifeId, union.Rank);
}
