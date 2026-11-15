using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.Seed;

/// <summary>
/// Seed dữ liệu mẫu theo "Mức 3": trục chính các vua + các nhánh lịch sử tiêu biểu.
/// Những người không đưa vào được đánh dấu HasHiddenChildren = true để UI hiện "…".
/// Chỉ chạy khi bảng Persons còn trống (idempotent).
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
        // Idempotent: dòng họ mẫu luôn ở trạng thái Draft để còn chỉnh sửa cây.
        // Tránh trường hợp DB cũ đã từng publish khiến cây bị khóa (409).
        var existingClan = await _context.Clans
            .FirstOrDefaultAsync(x => x.Name == ClanName, cancellationToken);
        if (existingClan is not null && existingClan.Status != ClanStatus.Draft)
        {
            existingClan.Status = ClanStatus.Draft;
            existingClan.PublishedAt = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (await _context.Persons.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var clan = existingClan;

        if (clan is null)
        {
            clan = new Clan
            {
                Name = ClanName,
                Description = "Phả hệ nhà Nguyễn (1802–1945).",
                OriginStory = "Triều Nguyễn là vương triều quân chủ cuối cùng của Việt Nam, do dòng họ Nguyễn Phúc sáng lập năm 1802.",
                // Draft để còn sửa cây (biên tập). Người duyệt bấm "Publish" khi đã xong.
                Status = ClanStatus.Draft,
                Visibility = ClanVisibility.Public
            };

            _context.Clans.Add(clan);
        }

        // ===== Đời 1 =====
        var giaLong = Root(clan.Id, "Nguyễn Phúc Ánh (Gia Long)", "Ánh",
            birth: SolarDate(1762, 2, 8), death: SolarDate(1820, 2, 3),
            portraitUrl: "/portraits/gia-long.jpg", hasHiddenChildren: true);

        // ===== Đời 2 (con Gia Long) =====
        var canh = Child(clan.Id, giaLong, "Nguyễn Phúc Cảnh (Hoàng tử Cảnh)", "Cảnh", 1,
            birth: SolarDate(1780, 4, 6), death: SolarDate(1801, 3, 20));
        var minhMang = Child(clan.Id, giaLong, "Nguyễn Phúc Đảm (Minh Mạng)", "Đảm", 2,
            birth: SolarDate(1791, 5, 25), death: SolarDate(1841, 1, 20),
            portraitUrl: "/portraits/minh-mang.jpg", hasHiddenChildren: true);

        // ===== Đời 3 (con Minh Mạng — trục chính + nhánh tiêu biểu) =====
        var thieuTri = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tông (Thiệu Trị)", "Miên Tông", 1,
            birth: SolarDate(1807, 6, 16), death: SolarDate(1847, 10, 4),
            portraitUrl: "/portraits/thieu-tri.jpg", hasHiddenChildren: true);
        var mienDinh = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Định (Thọ Xuân vương)", "Miên Định", 2,
            birth: SolarDate(1810, 8, 5), death: SolarDate(1886, 11, 5), hasHiddenChildren: true);
        var mienNghi = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Nghi (Ninh Thuận Quận vương)", "Miên Nghi", 3,
            birth: SolarDate(1810, 12, 30), death: SolarDate(1874, 8, 12), hasHiddenChildren: true);
        var mienTham = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Thẩm (Tùng Thiện vương)", "Miên Thẩm", 4,
            birth: SolarDate(1819, 12, 11), death: SolarDate(1870, 4, 30), hasHiddenChildren: true);
        var mienTrinh = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Trinh (Tuy Lý vương)", "Miên Trinh", 5,
            birth: SolarDate(1820, 2, 3), death: SolarDate(1897, 11, 18), hasHiddenChildren: true);
        var mienBao = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Bảo (Tương An Quận vương)", "Miên Bảo", 6,
            birth: SolarDate(1820, 5, 20), death: SolarDate(1854, 3, 8), hasHiddenChildren: true);
        var mienMat = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Mật (Quảng Ninh Quận vương)", "Miên Mật", 7,
            birth: SolarDate(1825, 8, 17), death: SolarDate(1847, 5, 23));
        var mienTuan = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tuấn (Hòa Thạnh vương)", "Miên Tuấn", 8,
            birth: SolarDate(1827, 6, 12), death: SolarDate(1907, 6, 22), hasHiddenChildren: true);

        // ===== Đời 4 (con Thiệu Trị — trục chính + nhánh tiêu biểu) =====
        var hongBao = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Bảo (An Phong Quận vương)", "Hồng Bảo", 1,
            birth: SolarDate(1825, 4, 19), death: SolarDate(1854, 1, 1));
        var tuDuc = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Nhậm (Tự Đức)", "Hồng Nhậm", 2,
            birth: SolarDate(1829, 9, 22), death: SolarDate(1883, 7, 19),
            portraitUrl: "/portraits/tu-duc.jpg");
        var hongPho = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Phó (Thái Thạnh Quận vương)", "Hồng Phó", 3,
            birth: SolarDate(1833, 4, 20), death: SolarDate(1890, 5, 8), hasHiddenChildren: true);
        var hongY = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Y (Thụy Thái vương)", "Hồng Y", 4,
            birth: SolarDate(1833, 9, 11), death: SolarDate(1877, 2, 23), hasHiddenChildren: true);
        var hongTo = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Tố (Hoằng Trị vương)", "Hồng Tố", 5,
            birth: SolarDate(1834, 10, 25), death: SolarDate(1922, 9, 18), hasHiddenChildren: true);
        var hongCai = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Cai (Kiên Thái vương)", "Hồng Cai", 6,
            hasHiddenChildren: true);
        var hiepHoa = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Dật (Hiệp Hòa)", "Hồng Dật", 7,
            birth: SolarDate(1847, 11, 1), death: SolarDate(1883, 11, 29),
            portraitUrl: "/portraits/hiep-hoa.jpg", hasHiddenChildren: true);

        // ===== Đời 5 =====
        var ducDuc = Child(clan.Id, hongY, "Nguyễn Phúc Ưng Chân (Dục Đức)", "Ưng Chân", 1,
            birth: SolarDate(1852, 2, 23), death: SolarDate(1883, 10, 6),
            portraitUrl: "/portraits/duc-duc.jpg", hasHiddenChildren: true);
        var dongKhanh = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Kỷ (Đồng Khánh)", "Ưng Kỷ", 1,
            birth: SolarDate(1864, 2, 19), death: SolarDate(1889, 1, 28),
            portraitUrl: "/portraits/dong-khanh.jpg", hasHiddenChildren: true);
        var kienPhuc = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Đăng (Kiến Phúc)", "Ưng Đăng", 2,
            birth: SolarDate(1869, 2, 12), death: SolarDate(1884, 7, 31),
            portraitUrl: "/portraits/kien-phuc.jpg");
        var hamNghi = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Lịch (Hàm Nghi)", "Ưng Lịch", 3,
            birth: SolarDate(1871, 8, 3), death: SolarDate(1944, 1, 14),
            portraitUrl: "/portraits/ham-nghi.jpg", hasHiddenChildren: true);

        // ===== Đời 6 =====
        var thanhThai = Child(clan.Id, ducDuc, "Nguyễn Phúc Bửu Lân (Thành Thái)", "Bửu Lân", 1,
            birth: SolarDate(1879, 3, 14), death: SolarDate(1954, 3, 24),
            portraitUrl: "/portraits/thanh-thai.jpg", hasHiddenChildren: true);
        var buuTan = Child(clan.Id, ducDuc, "Nguyễn Phúc Bửu Tán (Tuyên Hóa vương)", "Bửu Tán", 2,
            birth: SolarDate(1882, 1, 1), death: SolarDate(1941, 5, 8), hasHiddenChildren: true);
        var khaiDinh = Child(clan.Id, dongKhanh, "Nguyễn Phúc Bửu Đảo (Khải Định)", "Bửu Đảo", 1,
            birth: SolarDate(1885, 10, 8), death: SolarDate(1925, 11, 6),
            portraitUrl: "/portraits/khai-dinh.jpg");

        // ===== Đời 7 =====
        var duyTan = Child(clan.Id, thanhThai, "Nguyễn Phúc Vĩnh San (Duy Tân)", "Vĩnh San", 1,
            birth: SolarDate(1900, 9, 19), death: SolarDate(1945, 12, 26),
            portraitUrl: "/portraits/duy-tan.jpg", hasHiddenChildren: true);
        var baoDai = Child(clan.Id, khaiDinh, "Nguyễn Phúc Vĩnh Thụy (Bảo Đại)", "Vĩnh Thụy", 1,
            birth: SolarDate(1913, 10, 22), death: SolarDate(1997, 7, 31),
            portraitUrl: "/portraits/bao-dai.jpg", hasHiddenChildren: true);

        // ===== Đời 8 (con Bảo Đại) =====
        var baoLong = Child(clan.Id, baoDai, "Nguyễn Phúc Bảo Long (Hoàng thái tử)", "Bảo Long", 1,
            birth: SolarDate(1936, 1, 4), death: SolarDate(2007, 7, 28));
        var phuongMai = Child(clan.Id, baoDai, "Nguyễn Phúc Phương Mai", "Phương Mai", 2,
            birth: SolarDate(1937, 8, 21), death: SolarDate(2021, 3, 8));
        var phuongLien = Child(clan.Id, baoDai, "Nguyễn Phúc Phương Liên", "Phương Liên", 3,
            birth: SolarDate(1938, 11, 3));
        var phuongDung = Child(clan.Id, baoDai, "Nguyễn Phúc Phương Dung", "Phương Dung", 4,
            birth: SolarDate(1942, 2, 5));
        var baoThang = Child(clan.Id, baoDai, "Nguyễn Phúc Bảo Thắng", "Bảo Thắng", 5,
            birth: SolarDate(1943, 9, 30), death: SolarDate(2017, 3, 15));

        // Tự Đức không có con, nhận 3 cháu làm con nuôi.
        Adopt(tuDuc, ducDuc);
        Adopt(tuDuc, dongKhanh);
        Adopt(tuDuc, kienPhuc);

        await _context.SaveChangesAsync(cancellationToken);
        return 32;
    }

    private Person Root(
        Guid clanId,
        string fullName,
        string huyName,
        FlexibleDate? birth = null,
        FlexibleDate? death = null,
        string? portraitUrl = null,
        bool hasHiddenChildren = false)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death, portraitUrl, hasHiddenChildren);
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
        FlexibleDate? death = null,
        string? portraitUrl = null,
        bool hasHiddenChildren = false)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death, portraitUrl, hasHiddenChildren);
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
        FlexibleDate? death,
        string? portraitUrl,
        bool hasHiddenChildren)
        => new()
        {
            ClanId = clanId,
            FullName = fullName,
            HuyName = huyName,
            Gender = Gender.Male,
            BirthDate = birth,
            DeathDate = death,
            IsLiving = death is null && (birth?.Year ?? 0) >= 1930,
            PortraitUrl = portraitUrl,
            HasHiddenChildren = hasHiddenChildren,
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
