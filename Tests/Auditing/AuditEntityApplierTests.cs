using System.Text.Json;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Auditing;

namespace GiaPha.UnitTests.Auditing;

public sealed class AuditEntityApplierTests
{
    [Fact]
    public void Apply_ConvertsEnumAndFlexibleDate()
    {
        var person = new Person { Gender = Gender.Male };

        var before = new Dictionary<string, JsonElement>
        {
            ["gender"] = JsonSerializer.SerializeToElement("Female"),
            ["birthDate"] = JsonSerializer.SerializeToElement(new
            {
                year = 1990,
                month = 5,
                day = 15,
                calendar = "Lunar",
                isLeapMonth = false
            })
        };

        var applied = AuditEntityApplier.Apply(person, before);

        Assert.Equal(2, applied);
        Assert.Equal(Gender.Female, person.Gender);
        Assert.NotNull(person.BirthDate);
        Assert.Equal(1990, person.BirthDate!.Year);
        Assert.Equal(5, person.BirthDate.Month);
        Assert.Equal(15, person.BirthDate.Day);
        Assert.Equal(CalendarType.Lunar, person.BirthDate.Calendar);
    }

    [Fact]
    public void FindMismatches_ReturnsLabelsForChangedFields()
    {
        var person = new Person { FullName = "Nguyễn Văn B", Gender = Gender.Female };

        var after = new Dictionary<string, JsonElement>
        {
            ["fullName"] = JsonSerializer.SerializeToElement("Nguyễn Văn B"),
            ["gender"] = JsonSerializer.SerializeToElement("Male")
        };

        var mismatches = AuditEntityApplier.FindMismatches(person, after);

        var mismatch = Assert.Single(mismatches);
        Assert.Equal("Giới tính", mismatch);
    }
}
