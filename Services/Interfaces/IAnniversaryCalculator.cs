using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public sealed record AnniversaryOccurrence(Guid PersonId, string FullName, int Generation, Guid? BranchId,
    LunarDate OriginalDeathLunarDate, LunarDate ObservedLunarDate, DateOnly SolarDate, int YearsSinceDeath,
    bool IsAdjustedFromLeapMonth, bool IsAdjustedFromDay30To29);
public sealed record AnniversarySkipWarning(Guid PersonId, string FullName, string ReasonCode, string Message);
public sealed record PersonAnniversaryCalcResult(bool IsCalculable,
    IReadOnlyList<AnniversaryOccurrence> OccurrencesInSolarYear, AnniversarySkipWarning? Warning);
public interface IAnniversaryCalculator
{
    /// <summary>Chỉ trả lần sớm nhất; cần đủ các lần giỗ thì dùng CalculateForSolarYear.</summary>
    DateOnly? GetSolarDate(Person person, int solarYear);
    PersonAnniversaryCalcResult CalculateForSolarYear(Person person, int solarYear);
}
