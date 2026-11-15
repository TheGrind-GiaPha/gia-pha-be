using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Auditing;

namespace GiaPha.UnitTests.Auditing;

public sealed class AuditDiffFormatterTests
{
    [Fact]
    public void ToDto_FormatsClanNameChangeInVietnamese()
    {
        var log = new AuditLog
        {
            EntityType = "Clan",
            Action = AuditAction.Update,
            Before = """{"name":"Gia phả họ Nguyễn Hữu"}""",
            After = """{"name":"Gia phả họ Nguyễn Hữu 2"}"""
        };

        var dto = AuditDiffFormatter.ToDto(log);

        var change = Assert.Single(dto.Changes);
        Assert.Equal("name", change.Field);
        Assert.Equal("Tên dòng họ", change.Label);
        Assert.Equal("Gia phả họ Nguyễn Hữu", change.Before);
        Assert.Equal("Gia phả họ Nguyễn Hữu 2", change.After);
        Assert.Equal("Cập nhật dòng họ Gia phả họ Nguyễn Hữu 2", dto.Summary);
    }

    [Fact]
    public void ToDto_FormatsPersonEnumsAndLunarDate()
    {
        var log = new AuditLog
        {
            EntityType = "Person",
            Action = AuditAction.Update,
            Before = """{"gender":"Male","status":"Active"}""",
            After = """{"gender":"Female","status":"Deleted","birthDate":{"year":1990,"month":5,"day":15,"calendar":"Lunar","isLeapMonth":false}}"""
        };

        var dto = AuditDiffFormatter.ToDto(log);

        Assert.Equal("Cập nhật thành viên", dto.Summary);

        var gender = Assert.Single(dto.Changes, x => x.Field == "gender");
        Assert.Equal("Giới tính", gender.Label);
        Assert.Equal("Nam", gender.Before);
        Assert.Equal("Nữ", gender.After);

        var status = Assert.Single(dto.Changes, x => x.Field == "status");
        Assert.Equal("Đang hoạt động", status.Before);
        Assert.Equal("Đã xóa", status.After);

        var birthDate = Assert.Single(dto.Changes, x => x.Field == "birthDate");
        Assert.Equal("(trống)", birthDate.Before);
        Assert.Equal("15/05/1990 (âm lịch)", birthDate.After);
    }

    [Fact]
    public void ToDto_SkipsTechnicalFields()
    {
        var log = new AuditLog
        {
            EntityType = "Person",
            Action = AuditAction.Update,
            Before = """{"id":"6f9619ff-8b86-d011-b42d-00c04fc964ff","clanId":"6f9619ff-8b86-d011-b42d-00c04fc964ff","path":"/1/2/","version":1,"fullName":"A"}""",
            After = """{"id":"6f9619ff-8b86-d011-b42d-00c04fc964ff","clanId":"6f9619ff-8b86-d011-b42d-00c04fc964ff","path":"/1/2/","version":2,"fullName":"B"}"""
        };

        var dto = AuditDiffFormatter.ToDto(log);

        var change = Assert.Single(dto.Changes);
        Assert.Equal("fullName", change.Field);
    }

    [Fact]
    public void ToDto_SkipsFieldsThatAreEmptyOnBothSides()
    {
        var log = new AuditLog
        {
            EntityType = "Person",
            Action = AuditAction.Create,
            After = """{"fullName":"Nguyễn Văn A","tuName":null,"branchId":null}"""
        };

        var dto = AuditDiffFormatter.ToDto(log);

        var change = Assert.Single(dto.Changes);
        Assert.Equal("fullName", change.Field);
    }

    [Fact]
    public void ToDto_MarksOccurredAtAsUtc_WhenDatabaseReturnsUnspecified()
    {
        var log = new AuditLog
        {
            EntityType = "Person",
            Action = AuditAction.Update,
            OccurredAt = new DateTime(2026, 11, 15, 14, 55, 47, DateTimeKind.Unspecified)
        };

        var dto = AuditDiffFormatter.ToDto(log);

        Assert.Equal(DateTimeKind.Utc, dto.OccurredAt.Kind);
    }

    [Fact]
    public void ToDto_FormatsDeleteSnapshot()
    {
        var log = new AuditLog
        {
            EntityType = "Clan",
            Action = AuditAction.Delete,
            Before = """{"name":"Họ Trần","description":"Dòng họ Trần"}""",
            After = null
        };

        var dto = AuditDiffFormatter.ToDto(log);

        Assert.Equal("Xóa dòng họ Họ Trần", dto.Summary);
        Assert.Contains(dto.Changes, x => x.Field == "name" && x.After == "(trống)");
    }
}
