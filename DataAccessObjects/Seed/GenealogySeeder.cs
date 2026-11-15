using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.Seed;

/// <summary>
/// Seed dữ liệu mẫu: gia phả 13 đời vua triều Nguyễn (1802–1945).
/// Chỉ chạy khi bảng Persons còn trống (idempotent).
/// Ngày sinh/mất dùng dương lịch để đối chiếu chính xác.
/// </summary>
public sealed class GenealogySeeder
{
    private const string ClanName = "Nhà Nguyễn (Nguyễn Phúc tộc)";

    private readonly GiaPhaDbContext _context;

    public GenealogySeeder(GiaPhaDbContext context)
    {
        _context = context;
    }

    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Persons.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var clan = await _context.Clans
            .FirstOrDefaultAsync(x => x.Name == ClanName, cancellationToken);

        if (clan is null)
        {
            clan = new Clan
            {
                Name = ClanName,
                Description = "Gia phả 13 đời vua triều Nguyễn, từ Gia Long đến Bảo Đại (1802–1945).",
                OriginStory = "Triều Nguyễn là vương triều quân chủ cuối cùng của Việt Nam, do dòng họ Nguyễn Phúc sáng lập năm 1802.",
                Status = ClanStatus.Published,
                PublishedAt = DateTime.UtcNow
            };

            _context.Clans.Add(clan);
        }

        // Đời 1
        var giaLong = Root(clan.Id, "Nguyễn Phúc Ánh (Gia Long)", "Ánh",
            birth: SolarDate(1762, 2, 8), death: SolarDate(1820, 2, 3));

        // Đời 2
        var minhMang = Child(clan.Id, giaLong, "Nguyễn Phúc Đảm (Minh Mạng)", "Đảm", 1,
            birth: SolarDate(1791, 5, 25), death: SolarDate(1841, 1, 20));

        // Đời 3
        var thieuTri = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tông (Thiệu Trị)", "Miên Tông", 1,
            birth: SolarDate(1807, 6, 16), death: SolarDate(1847, 10, 4));

        // Đời 4: các con của Thiệu Trị
        var tuDuc = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Nhậm (Tự Đức)", "Hồng Nhậm", 1,
            birth: SolarDate(1829, 9, 22), death: SolarDate(1883, 7, 19));
        var hongY = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Y (Thụy Thái vương)", "Hồng Y", 2);
        var hongCai = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Cai (Kiên Thái vương)", "Hồng Cai", 3);
        var hiepHoa = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Dật (Hiệp Hòa)", "Hồng Dật", 4,
            birth: SolarDate(1847, 11, 1), death: SolarDate(1883, 11, 29));

        // Đời 5
        var ducDuc = Child(clan.Id, hongY, "Nguyễn Phúc Ưng Chân (Dục Đức)", "Ưng Chân", 1,
            birth: SolarDate(1852, 2, 23), death: SolarDate(1883, 10, 6));
        var dongKhanh = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Kỷ (Đồng Khánh)", "Ưng Kỷ", 1,
            birth: SolarDate(1864, 2, 19), death: SolarDate(1889, 1, 28));
        var kienPhuc = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Đăng (Kiến Phúc)", "Ưng Đăng", 2,
            birth: SolarDate(1869, 2, 12), death: SolarDate(1884, 7, 31));
        var hamNghi = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Lịch (Hàm Nghi)", "Ưng Lịch", 3,
            birth: SolarDate(1871, 8, 3), death: SolarDate(1944, 1, 14));

        // Đời 6
        var thanhThai = Child(clan.Id, ducDuc, "Nguyễn Phúc Bửu Lân (Thành Thái)", "Bửu Lân", 1,
            birth: SolarDate(1879, 3, 14), death: SolarDate(1954, 3, 24));
        var khaiDinh = Child(clan.Id, dongKhanh, "Nguyễn Phúc Bửu Đảo (Khải Định)", "Bửu Đảo", 1,
            birth: SolarDate(1885, 10, 8), death: SolarDate(1925, 11, 6));

        // Đời 7
        var duyTan = Child(clan.Id, thanhThai, "Nguyễn Phúc Vĩnh San (Duy Tân)", "Vĩnh San", 1,
            birth: SolarDate(1900, 9, 19), death: SolarDate(1945, 12, 26));
        var baoDai = Child(clan.Id, khaiDinh, "Nguyễn Phúc Vĩnh Thụy (Bảo Đại)", "Vĩnh Thụy", 1,
            birth: SolarDate(1913, 10, 22), death: SolarDate(1997, 7, 30));

        // Tự Đức không có con, nhận 3 cháu làm con nuôi.
        Adopt(tuDuc, ducDuc);
        Adopt(tuDuc, dongKhanh);
        Adopt(tuDuc, kienPhuc);

        await _context.SaveChangesAsync(cancellationToken);
        return 15;
    }

    private Person Root(
        Guid clanId,
        string fullName,
        string huyName,
        FlexibleDate? birth = null,
        FlexibleDate? death = null)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death);
        person.Generation = 1;
        person.Path = $"/{person.Id}/";

        _context.Persons.Add(person);
        return person;
    }

    private Person Child(
        Guid clanId,
        Person father,
        string fullName,
        string huyName,
        int siblingOrder,
        FlexibleDate? birth = null,
        FlexibleDate? death = null)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death);
        person.Generation = father.Generation + 1;
        person.Path = father.Path + person.Id + "/";

        _context.Persons.Add(person);
        _context.ParentChild.Add(new ParentChild
        {
            ParentId = father.Id,
            ChildId = person.Id,
            RelationType = RelationType.Biological,
            SiblingOrder = siblingOrder
        });

        return person;
    }

    private void Adopt(Person parent, Person child)
    {
        _context.ParentChild.Add(new ParentChild
        {
            ParentId = parent.Id,
            ChildId = child.Id,
            RelationType = RelationType.Adopted,
            SiblingOrder = 0
        });
    }

    private static Person CreatePerson(
        Guid clanId,
        string fullName,
        string huyName,
        FlexibleDate? birth,
        FlexibleDate? death)
        => new()
        {
            ClanId = clanId,
            FullName = fullName,
            HuyName = huyName,
            Gender = Gender.Male,
            BirthDate = birth,
            DeathDate = death,
            IsLiving = false,
            Status = PersonStatus.Active,
            PrivacyLevel = PrivacyLevel.Public,
            Version = 1
        };

    private static FlexibleDate? SolarDate(int? year, int? month, int? day)
        => year is null
            ? null
            : new FlexibleDate
            {
                Year = year,
                Month = month,
                Day = day,
                Calendar = CalendarType.Solar,
                IsLeapMonth = false
            };
}
