using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Context;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.Seed;

/// <summary>
/// Seed dữ liệu mẫu: gia phả nhà Nguyễn (13 đời vua + hoàng tử/công chúa tiêu biểu).
/// Chỉ chạy khi bảng Persons còn trống (idempotent).
/// Ngày sinh/mất dùng dương lịch; nguồn: Wikipedia + Nguyễn Phúc tộc thế phả.
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
                Description = "Gia phả triều Nguyễn (1802–1945) cùng các hoàng tử, công chúa tiêu biểu.",
                OriginStory = "Triều Nguyễn là vương triều quân chủ cuối cùng của Việt Nam, do dòng họ Nguyễn Phúc sáng lập năm 1802.",
                Status = ClanStatus.Published,
                PublishedAt = DateTime.UtcNow
            };

            _context.Clans.Add(clan);
        }

        // ===== Đời 1 =====
        var giaLong = Root(clan.Id, "Nguyễn Phúc Ánh (Gia Long)", "Ánh",
            birth: SolarDate(1762, 2, 8), death: SolarDate(1820, 2, 3), portraitUrl: "/portraits/gia-long.jpg");

        // ===== Đời 2 (con Gia Long) =====
        var canh = Child(clan.Id, giaLong, "Nguyễn Phúc Cảnh (Hoàng tử Cảnh)", "Cảnh", 1,
            birth: SolarDate(1780, 4, 6), death: SolarDate(1801, 3, 20));
        var minhMang = Child(clan.Id, giaLong, "Nguyễn Phúc Đảm (Minh Mạng)", "Đảm", 2,
            birth: SolarDate(1791, 5, 25), death: SolarDate(1841, 1, 20), portraitUrl: "/portraits/minh-mang.jpg");

        // ===== Đời 3 (con Minh Mạng) =====
        var thieuTri = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tông (Thiệu Trị)", "Miên Tông", 1,
            birth: SolarDate(1807, 6, 16), death: SolarDate(1847, 10, 4), portraitUrl: "/portraits/thieu-tri.jpg");
        var mienDinh = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Định (Thọ Xuân vương)", "Miên Định", 2,
            birth: SolarDate(1810, 8, 5), death: SolarDate(1886, 11, 5));
        var mienNghi = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Nghi (Ninh Thuận Quận vương)", "Miên Nghi", 3,
            birth: SolarDate(1810, 12, 30), death: SolarDate(1874, 8, 12));
        var mienHoanh = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Hoành (Vĩnh Tường Quận vương)", "Miên Hoành", 4,
            birth: SolarDate(1811, 7, 12), death: SolarDate(1835, 11, 23));
        var mienAo = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Áo (Phú Bình Quận vương)", "Miên Áo", 5,
            birth: SolarDate(1817, 1, 4), death: SolarDate(1865, 2, 1));
        var mienThan = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Thần (Nghi Hòa Quận công)", "Miên Thần", 6,
            birth: SolarDate(1817, 2, 16), death: SolarDate(1878, 10, 7));
        var mienPhu = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Phú (Phù Mỹ Quận công)", "Miên Phú", 7,
            birth: SolarDate(1817, 2, 28), death: SolarDate(1885, 4, 15));
        var mienThu = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Thủ (Hàm Thuận công)", "Miên Thủ", 8,
            birth: SolarDate(1819, 3, 5), death: SolarDate(1859, 9, 24));
        var mienTham = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Thẩm (Tùng Thiện vương)", "Miên Thẩm", 9,
            birth: SolarDate(1819, 12, 11), death: SolarDate(1870, 4, 30));
        var mienTrinh = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Trinh (Tuy Lý vương)", "Miên Trinh", 10,
            birth: SolarDate(1820, 2, 3), death: SolarDate(1897, 11, 18));
        var mienBao = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Bảo (Tương An Quận vương)", "Miên Bảo", 11,
            birth: SolarDate(1820, 5, 20), death: SolarDate(1854, 3, 8));
        var mienTru = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Trữ (Tuân Quốc công)", "Miên Trữ", 12,
            birth: SolarDate(1820, 7, 11), death: SolarDate(1890, 8, 20));
        var mienVu = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Vũ (Lạc Hóa Quận công)", "Miên Vũ", 13,
            birth: SolarDate(1822, 2, 21), death: SolarDate(1849, 11, 17));
        var mienTong = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tống (Hà Thanh Quận công)", "Miên Tống", 14,
            birth: SolarDate(1822, 3, 17), death: SolarDate(1858, 3, 2));
        var mienTe = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tể (Nghĩa Quốc công)", "Miên Tể", 15,
            birth: SolarDate(1822, 10, 21), death: SolarDate(1844, 12, 5));
        var mienTich = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tích (Trấn Man Quận công)", "Miên Tích", 16,
            birth: SolarDate(1823, 3, 8), death: SolarDate(1866, 8, 5));
        var mienCung = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Cung (Sơn Định Quận công)", "Miên Cung", 17,
            birth: SolarDate(1824, 1, 17), death: SolarDate(1849, 10, 2));
        var mienPhong = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Phong (Tân Bình Quận công)", "Miên Phong", 18,
            birth: SolarDate(1824, 5, 16), death: SolarDate(1860, 10, 30));
        var mienLieu = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Liêu (Quỳ Châu Quận công)", "Miên Liêu", 19,
            birth: SolarDate(1824, 8, 23), death: SolarDate(1881, 1, 1));
        var mienMat = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Mật (Quảng Ninh Quận vương)", "Miên Mật", 20,
            birth: SolarDate(1825, 8, 17), death: SolarDate(1847, 5, 23));
        var mienLuong = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Lương (Sơn Tĩnh Quận công)", "Miên Lương", 21,
            birth: SolarDate(1826, 2, 27), death: SolarDate(1863, 8, 24));
        var mienGia = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Gia (Quảng Biên Quận công)", "Miên Gia", 22,
            birth: SolarDate(1826, 5, 22), death: SolarDate(1875, 7, 20));
        var mienKhoan = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Khoan (Lạc Biên Quận công)", "Miên Khoan", 23,
            birth: SolarDate(1826, 7, 8), death: SolarDate(1863, 9, 2));
        var mienTuan = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tuấn (Hòa Thạnh vương)", "Miên Tuấn", 24,
            birth: SolarDate(1827, 6, 12), death: SolarDate(1907, 6, 22));
        var mienQuan = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Quân (Hòa Quốc công)", "Miên Quân", 25,
            birth: SolarDate(1828, 8, 31), death: SolarDate(1863, 8, 17));
        var mienKhap = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Kháp (Tuy An Quận công)", "Miên Kháp", 26,
            birth: SolarDate(1828, 10, 5), death: SolarDate(1893, 7, 14));
        var mienTang = Child(clan.Id, minhMang, "Nguyễn Phúc Miên Tằng (Hải Quốc công)", "Miên Tằng", 27,
            birth: SolarDate(1828, 10, 27), death: SolarDate(1896, 4, 16));

        // ===== Đời 4 (con Thiệu Trị) =====
        var hongBao = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Bảo (An Phong Quận vương)", "Hồng Bảo", 1,
            birth: SolarDate(1825, 4, 19), death: SolarDate(1854, 1, 1));
        var tuDuc = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Nhậm (Tự Đức)", "Hồng Nhậm", 2,
            birth: SolarDate(1829, 9, 22), death: SolarDate(1883, 7, 19), portraitUrl: "/portraits/tu-duc.jpg");
        var hongPho = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Phó (Thái Thạnh Quận vương)", "Hồng Phó", 3,
            birth: SolarDate(1833, 4, 20), death: SolarDate(1890, 5, 8));
        var hongY = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Y (Thụy Thái vương)", "Hồng Y", 4,
            birth: SolarDate(1833, 9, 11), death: SolarDate(1877, 2, 23));
        var hongTo = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Tố (Hoằng Trị vương)", "Hồng Tố", 5,
            birth: SolarDate(1834, 10, 25), death: SolarDate(1922, 9, 18));
        var hongCai = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Cai (Kiên Thái vương)", "Hồng Cai", 6);
        var hiepHoa = Child(clan.Id, thieuTri, "Nguyễn Phúc Hồng Dật (Hiệp Hòa)", "Hồng Dật", 7,
            birth: SolarDate(1847, 11, 1), death: SolarDate(1883, 11, 29), portraitUrl: "/portraits/hiep-hoa.jpg");

        // ===== Đời 5 =====
        var ducDuc = Child(clan.Id, hongY, "Nguyễn Phúc Ưng Chân (Dục Đức)", "Ưng Chân", 1,
            birth: SolarDate(1852, 2, 23), death: SolarDate(1883, 10, 6), portraitUrl: "/portraits/duc-duc.jpg");
        var dongKhanh = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Kỷ (Đồng Khánh)", "Ưng Kỷ", 1,
            birth: SolarDate(1864, 2, 19), death: SolarDate(1889, 1, 28), portraitUrl: "/portraits/dong-khanh.jpg");
        var kienPhuc = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Đăng (Kiến Phúc)", "Ưng Đăng", 2,
            birth: SolarDate(1869, 2, 12), death: SolarDate(1884, 7, 31), portraitUrl: "/portraits/kien-phuc.jpg");
        var hamNghi = Child(clan.Id, hongCai, "Nguyễn Phúc Ưng Lịch (Hàm Nghi)", "Ưng Lịch", 3,
            birth: SolarDate(1871, 8, 3), death: SolarDate(1944, 1, 14), portraitUrl: "/portraits/ham-nghi.jpg");

        // ===== Đời 6 =====
        var thanhThai = Child(clan.Id, ducDuc, "Nguyễn Phúc Bửu Lân (Thành Thái)", "Bửu Lân", 1,
            birth: SolarDate(1879, 3, 14), death: SolarDate(1954, 3, 24), portraitUrl: "/portraits/thanh-thai.jpg");
        var buuTan = Child(clan.Id, ducDuc, "Nguyễn Phúc Bửu Tán (Tuyên Hóa vương)", "Bửu Tán", 2,
            birth: SolarDate(1882, 1, 1), death: SolarDate(1941, 5, 8));
        var khaiDinh = Child(clan.Id, dongKhanh, "Nguyễn Phúc Bửu Đảo (Khải Định)", "Bửu Đảo", 1,
            birth: SolarDate(1885, 10, 8), death: SolarDate(1925, 11, 6), portraitUrl: "/portraits/khai-dinh.jpg");

        // ===== Đời 7 =====
        var duyTan = Child(clan.Id, thanhThai, "Nguyễn Phúc Vĩnh San (Duy Tân)", "Vĩnh San", 1,
            birth: SolarDate(1900, 9, 19), death: SolarDate(1945, 12, 26), portraitUrl: "/portraits/duy-tan.jpg");
        var baoDai = Child(clan.Id, khaiDinh, "Nguyễn Phúc Vĩnh Thụy (Bảo Đại)", "Vĩnh Thụy", 1,
            birth: SolarDate(1913, 10, 22), death: SolarDate(1997, 7, 31), portraitUrl: "/portraits/bao-dai.jpg");

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
        return 51;
    }

    private Person Root(
        Guid clanId,
        string fullName,
        string huyName,
        FlexibleDate? birth = null,
        FlexibleDate? death = null,
        string? portraitUrl = null)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death, portraitUrl);
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
        string? portraitUrl = null)
    {
        var person = CreatePerson(clanId, fullName, huyName, birth, death, portraitUrl);
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
        string? portraitUrl)
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
