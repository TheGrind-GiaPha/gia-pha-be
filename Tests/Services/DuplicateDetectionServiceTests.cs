using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Implementations;
using Xunit;

namespace GiaPha.UnitTests.Services;

public sealed class DuplicateDetectionServiceTests
{
    private readonly DuplicateDetectionService _service = new();

    [Theory]
    [InlineData("Nguyễn Văn Ánh", "nguyen van anh")]
    [InlineData("Lê Thị Mỹ Duyên", "le thi my duyen")]
    [InlineData("Đặng Đức Đạt", "dang duc dat")]
    public void RemoveDiacritics_ShouldNormalizeCorrectly(string input, string expected)
    {
        var result = _service.RemoveDiacritics(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CheckDuplicate_WhenNameMatches_BirthYearDiffWithin1_AndFatherMatches_ShouldReturnTrue()
    {
        var fatherId = Guid.NewGuid();
        var existingPerson = new Person
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyễn Văn Đạt",
            BirthDate = new FlexibleDate { Year = 1980 },
            Generation = 2
        };

        var father = new Person
        {
            Id = fatherId,
            FullName = "Nguyễn Văn Khởi",
            BirthDate = new FlexibleDate { Year = 1950 },
            Generation = 1
        };

        var existingPersons = new List<Person> { father, existingPerson };
        var parentChildMap = new Dictionary<Guid, Guid> { { existingPerson.Id, fatherId } };

        var (isDup, matchedId, warn) = _service.CheckDuplicate(
            candidateFullName: "Nguyễn Văn Đạt",
            candidateBirthYear: 1981, // Lệch 1 năm
            candidateFatherName: "Nguyễn Văn Khởi",
            candidateFatherId: null,
            existingPersons: existingPersons,
            existingParentChildMap: parentChildMap);

        Assert.True(isDup);
        Assert.Equal(existingPerson.Id, matchedId);
        Assert.Contains("Nghi trùng", warn);
    }

    [Fact]
    public void CheckDuplicate_WhenBirthYearDiffGreaterThan1_ShouldReturnFalse()
    {
        var fatherId = Guid.NewGuid();
        var existingPerson = new Person
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyễn Văn Đạt",
            BirthDate = new FlexibleDate { Year = 1980 }
        };

        var existingPersons = new List<Person> { existingPerson };
        var parentChildMap = new Dictionary<Guid, Guid> { { existingPerson.Id, fatherId } };

        var (isDup, matchedId, _) = _service.CheckDuplicate(
            candidateFullName: "Nguyễn Văn Đạt",
            candidateBirthYear: 1985, // Lệch 5 năm
            candidateFatherName: "Nguyễn Văn Khởi",
            candidateFatherId: null,
            existingPersons: existingPersons,
            existingParentChildMap: parentChildMap);

        Assert.False(isDup);
        Assert.Null(matchedId);
    }
}
