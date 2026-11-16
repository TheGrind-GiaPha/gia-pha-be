using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Services.Implementations;

namespace GiaPha.UnitTests.Services;

public class LunarConverterTests
{
    private readonly LunarConverter _converter = new();

    // Tết 2020–2025: https://calendarific.com/holiday/vietnam/tet-new-year
    // Tết 2026: https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2026e.pdf
    [Theory]
    [InlineData(2020, 1, 25)] [InlineData(2021, 2, 12)] [InlineData(2022, 2, 1)]
    [InlineData(2023, 1, 22)] [InlineData(2024, 2, 10)] [InlineData(2025, 1, 29)] [InlineData(2026, 2, 17)]
    public void Tet_IndependentDates(int year, int month, int day)
    {
        var solar = new DateOnly(year, month, day);
        var lunar = new LunarDate(1, 1, year, false);
        Assert.Equal(lunar, _converter.SolarToLunar(solar));
        Assert.Equal(solar, _converter.LunarToSolar(lunar));
    }

    // 10/3 2024: https://mnhoangvanthu.haiphong.edu.vn/thong-bao/thong-bao-nghi-ngay-gio-to-hung-vuong-nam-2024/ct/2500/183235
    // 15/4 2024: https://phathoc.vn/2pdf/hue-dai-le-phat-dan-pl-2568-dl-2024
    // 10/3 2025: https://vanphong.langson.gov.vn/thong-bao/lich-truc-nghi-le-gio-to-hung-vuong-10-3-am-lich-ngay-07-4-2025.html
    [Theory]
    [InlineData(2024, 4, 18, 10, 3)] [InlineData(2024, 5, 22, 15, 4)] [InlineData(2025, 4, 7, 10, 3)]
    public void Festivals_IndependentDates(int year, int month, int day, int lunarDay, int lunarMonth)
    {
        var lunar = new LunarDate(lunarDay, lunarMonth, year, false);
        var solar = new DateOnly(year, month, day);
        Assert.Equal(lunar, _converter.SolarToLunar(solar));
        Assert.Equal(solar, _converter.LunarToSolar(lunar));
    }

    // Hong Kong Observatory published calendar tables; these specific modern dates agree with Vietnamese UTC+7.
    // https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2020e.pdf
    // https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2023e.pdf
    // https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2025e.pdf
    [Theory]
    [InlineData(2020, 4, 5, 23, 6, 20)] [InlineData(2023, 2, 3, 22, 4, 19)] [InlineData(2025, 6, 7, 25, 8, 22)]
    public void LeapMonths_IndependentDates(int year, int leapMonth, int startMonth, int startDay, int endMonth, int endDay)
    {
        Assert.Equal(leapMonth, _converter.GetLeapMonth(year));
        Assert.Equal(29, _converter.GetLunarMonthDays(year, leapMonth, true));
        Assert.Equal(new LunarDate(1, leapMonth, year, true), _converter.SolarToLunar(new(year, startMonth, startDay)));
        Assert.Equal(new LunarDate(29, leapMonth, year, true), _converter.SolarToLunar(new(year, endMonth, endDay)));
        Assert.Equal(new DateOnly(year, startMonth, startDay), _converter.LunarToSolar(1, leapMonth, year, true));
        Assert.Equal(new DateOnly(year, endMonth, endDay), _converter.LunarToSolar(29, leapMonth, year, true));
    }
    // https://www.hko.gov.hk/en/gts/time/calendar/pdf/files/2025e.pdf
    [Fact] public void Eve_2025() => Assert.Equal(new LunarDate(29, 12, 2024, false), _converter.SolarToLunar(new(2025, 1, 28)));

    [Fact]
    public void RoundTrip_EverySolarDayInSupportedRange()
    {
        var last = new DateOnly(2199, 12, 31);
        for (var day = new DateOnly(1800, 1, 1); ; day = day.AddDays(1))
        {
            Assert.Equal(day, _converter.LunarToSolar(_converter.SolarToLunar(day)));
            if (day == last) break;
        }
        Assert.Equal(new DateOnly(1800, 1, 1), _converter.LunarToSolar(7, 12, 1799));
    }
    [Theory]
    [InlineData(30, 12, 2024, false, "INVALID_LUNAR_DAY")]
    [InlineData(30, 2, 2025, false, "INVALID_LUNAR_DAY")]
    [InlineData(0, 1, 2025, false, "INVALID_LUNAR_DAY")]
    [InlineData(31, 1, 2025, false, "INVALID_LUNAR_DAY")]
    [InlineData(1, 0, 2025, false, "INVALID_LUNAR_MONTH")]
    [InlineData(15, 3, 2025, true, "INVALID_LEAP_MONTH")]
    [InlineData(1, 1, 2200, false, "YEAR_OUT_OF_RANGE")]
    [InlineData(6, 12, 1799, false, "YEAR_OUT_OF_RANGE")]
    public void InvalidLunarDates_HaveCorrectCode(int day, int month, int year, bool leap, string code) =>
        Assert.Equal(code, Assert.Throws<DomainValidationException>(() => _converter.LunarToSolar(day, month, year, leap)).Code);
    [Fact] public void SolarOutsideRange_HasCorrectCode() => Assert.Equal("YEAR_OUT_OF_RANGE", Assert.Throws<DomainValidationException>(() => _converter.SolarToLunar(new(1799, 12, 31))).Code);

    [Fact]
    public void Format_PreservesPartialAndInvalidInputs()
    {
        Assert.Equal("", _converter.FormatLunarDate(new()));
        Assert.Equal("05/1990 (DL)", _converter.FormatLunarDate(new() { Month = 5, Year = 1990 }));
        Assert.Equal("1990 (DL)", _converter.FormatLunarDate(new() { Year = 1990 }));
        Assert.Equal("19/12 (ÂL)", _converter.FormatLunarDate(new() { Day = 19, Month = 12, Calendar = CalendarType.Lunar }));
        Assert.Equal("08/02/1762 (DL)", _converter.FormatLunarDate(new() { Day = 8, Month = 2, Year = 1762 }));
        Assert.Equal("31/02/2024 (DL)", _converter.FormatLunarDate(new() { Day = 31, Month = 2, Year = 2024 }));
        Assert.Equal("30/12/2024 (ÂL)", _converter.FormatLunarDate(new() { Day = 30, Month = 12, Year = 2024, Calendar = CalendarType.Lunar }));
        Assert.Equal("15/04 nhuận/2020 (ÂL)", _converter.FormatLunarDate(new() { Day = 15, Month = 4, Year = 2020, Calendar = CalendarType.Lunar, IsLeapMonth = true }));
        Assert.Equal("19/12/1819 (ÂL)", _converter.FormatLunarDate(new() { Day = 3, Month = 2, Year = 1820 }));
    }
}
