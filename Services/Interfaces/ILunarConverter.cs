using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface ILunarConverter
{
    LunarDate SolarToLunar(DateOnly solarDate);
    DateOnly LunarToSolar(int lunarDay, int lunarMonth, int lunarYear, bool isLeapMonth = false);
    DateOnly LunarToSolar(LunarDate lunarDate);
    int GetLunarMonthDays(int lunarYear, int lunarMonth, bool isLeapMonth = false);
    int? GetLeapMonth(int lunarYear);
    string FormatLunarDate(FlexibleDate date);
}
