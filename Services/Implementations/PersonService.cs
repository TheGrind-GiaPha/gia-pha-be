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

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Họ tên người không được để trống.", nameof(name));
        }

        return normalized;
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

    private static PersonDto Map(Person person)
        => new(person.Id, person.ClanId, person.FullName, person.Gender, person.Generation, person.Path);

    private static UnionDto Map(Union union)
        => new(union.Id, union.HusbandId, union.WifeId, union.Rank);
}
