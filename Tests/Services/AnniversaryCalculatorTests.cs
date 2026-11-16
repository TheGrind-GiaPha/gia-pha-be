using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Implementations;

namespace GiaPha.UnitTests.Services;

public class AnniversaryCalculatorTests
{
    private readonly AnniversaryCalculator _calculator = new(new LunarConverter());
    private static Person Person(int y, int m, int d, CalendarType calendar = CalendarType.Solar, bool leap = false) => new()
    {
        Id = Guid.NewGuid(), FullName = "Test", IsLiving = false, Generation = 3,
        DeathDate = new() { Year = y, Month = m, Day = d, Calendar = calendar, IsLeapMonth = leap }
    };
    // Death date from Nguyễn Phúc Tộc Thế Phả: https://www.sggp.org.vn/vua-gia-long-va-vua-minh-mang-mat-vao-nam-nao-post122153.html
    // Corresponding modern lunar dates: https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2025e.pdf and 2026e.pdf
    [Fact]
    public void GiaLong_LunarDeathAndCrossYearAnniversaries()
    {
        var person = Person(1820, 2, 3);
        var a = Assert.Single(_calculator.CalculateForSolarYear(person, 2025).OccurrencesInSolarYear);
        Assert.Equal(new LunarDate(19, 12, 1819, false), a.OriginalDeathLunarDate);
        Assert.Equal(new DateOnly(2025, 1, 18), a.SolarDate);
        var b = Assert.Single(_calculator.CalculateForSolarYear(person, 2026).OccurrencesInSolarYear);
        Assert.Equal(new LunarDate(19, 12, 2025, false), b.ObservedLunarDate);
        Assert.Equal(new DateOnly(2026, 2, 6), b.SolarDate);
        Assert.Equal(206, b.YearsSinceDeath);
    }
    // Independent historical account: https://scholar.dlu.edu.vn/thuvienso/bitstream/DLU123456789/187558/1/CVv443S612022005.pdf
    // Modern reference: https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2024e.pdf and 2025e.pdf
    [Fact]
    public void MienTham_Day30_AdjustedOnlyInShortMonth()
    {
        var person = Person(1870, 4, 30);
        var a = Assert.Single(_calculator.CalculateForSolarYear(person, 2024).OccurrencesInSolarYear);
        Assert.Equal(new LunarDate(30, 3, 1870, false), a.OriginalDeathLunarDate);
        Assert.Equal(new DateOnly(2024, 5, 7), a.SolarDate);
        Assert.True(a.IsAdjustedFromDay30To29);
        var b = Assert.Single(_calculator.CalculateForSolarYear(person, 2025).OccurrencesInSolarYear);
        Assert.Equal(new DateOnly(2025, 4, 27), b.SolarDate);
        Assert.False(b.IsAdjustedFromDay30To29);
    }
    // 15/4/2025 lunar: https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2025e.pdf
    [Fact]
    public void LeapDeath_UsesRegularMonth()
    {
        var result = Assert.Single(_calculator.CalculateForSolarYear(Person(2020, 4, 15, CalendarType.Lunar, true), 2025).OccurrencesInSolarYear);
        Assert.Equal(new DateOnly(2025, 5, 12), result.SolarDate);
        Assert.True(result.IsAdjustedFromLeapMonth);
        Assert.False(result.ObservedLunarDate.IsLeapMonth);
    }
    // https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/1902e.pdf: 01/12 lunar occurs twice in 1902.
    [Fact]
    public void TwoOccurrences_GetSolarDateReturnsEarliest()
    {
        var person = Person(1900, 12, 1, CalendarType.Lunar);
        var dates = _calculator.CalculateForSolarYear(person, 1902).OccurrencesInSolarYear.Select(x => x.SolarDate).ToArray();
        Assert.Equal(new[] { new DateOnly(1902, 1, 10), new DateOnly(1902, 12, 30) }, dates);
        Assert.Equal(dates[0], _calculator.GetSolarDate(person, 1902));
    }
    [Theory] [InlineData(2023)] [InlineData(2024)]
    public void NoAnniversaryInOrBeforeDeathYear(int year) => Assert.Null(_calculator.GetSolarDate(Person(2024, 5, 10, CalendarType.Lunar), year));
    [Fact]
    public void SkippedPeopleAndWarnings()
    {
        var living = Person(2000, 1, 1); living.DeathDate = null; living.IsLiving = true;
        Assert.Null(_calculator.CalculateForSolarYear(living, 2025).Warning);
        living.DeathDate = new FlexibleDate();
        Assert.Equal("INCONSISTENT_LIVING_STATUS", _calculator.CalculateForSolarYear(living, 2025).Warning?.ReasonCode);
        living.Status = PersonStatus.Deleted;
        Assert.Null(_calculator.CalculateForSolarYear(living, 2025).Warning);
        var missing = new Person { IsLiving = false };
        Assert.Equal("MISSING_DEATH_DATE", _calculator.CalculateForSolarYear(missing, 2025).Warning?.ReasonCode);
        missing.DeathDate = new() { Year = 1884, Month = 7 };
        Assert.Equal("INCOMPLETE_DEATH_DATE", _calculator.CalculateForSolarYear(missing, 2025).Warning?.ReasonCode);
        Assert.False(_calculator.CalculateForSolarYear(Person(1762, 2, 8), 2025).IsCalculable);
        Assert.False(_calculator.CalculateForSolarYear(Person(1799, 12, 6, CalendarType.Lunar), 2025).IsCalculable);
    }
}
