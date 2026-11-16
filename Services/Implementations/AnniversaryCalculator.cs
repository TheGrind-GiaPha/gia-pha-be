using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class AnniversaryCalculator(ILunarConverter lunarConverter) : IAnniversaryCalculator
{
    /// <summary>Chỉ trả lần sớm nhất; cần đủ các lần giỗ thì dùng CalculateForSolarYear.</summary>
    public DateOnly? GetSolarDate(Person person, int solarYear) =>
        CalculateForSolarYear(person, solarYear).OccurrencesInSolarYear.FirstOrDefault()?.SolarDate;

    public PersonAnniversaryCalcResult CalculateForSolarYear(Person person, int solarYear)
    {
        static PersonAnniversaryCalcResult Skip() => new(false, Array.Empty<AnniversaryOccurrence>(), null);
        PersonAnniversaryCalcResult Warn(string code, string message) => new(false, Array.Empty<AnniversaryOccurrence>(), new(person.Id, person.FullName, code, message));
        if (person.Status == PersonStatus.Deleted) return Skip();
        if (person.IsLiving) return person.DeathDate is null ? Skip() : Warn("INCONSISTENT_LIVING_STATUS", "Người còn sống có ngày mất.");
        var date = person.DeathDate;
        if (date is null) return Warn("MISSING_DEATH_DATE", "Thiếu ngày mất.");
        if (!date.Year.HasValue || !date.Month.HasValue || !date.Day.HasValue) return Warn("INCOMPLETE_DEATH_DATE", "Ngày mất chưa đầy đủ.");
        if (solarYear is < 1800 or > 2199) throw new DomainValidationException("YEAR_OUT_OF_RANGE", "Năm dương lịch nằm ngoài khoảng hỗ trợ.");
        LunarDate original;
        DateOnly death;
        if (date.Calendar == CalendarType.Solar)
        {
            death = new DateOnly(date.Year.Value, date.Month.Value, date.Day.Value);
            if (death.Year < 1800) return Skip();
            original = lunarConverter.SolarToLunar(death);
        }
        else
        {
            original = new LunarDate(date.Day.Value, date.Month.Value, date.Year.Value, date.IsLeapMonth);
            try { death = lunarConverter.LunarToSolar(original); }
            catch (DomainValidationException ex) when (ex.Code == "YEAR_OUT_OF_RANGE" && original.Year < 1800) { return Skip(); }
        }
        var occurrences = new List<AnniversaryOccurrence>();
        if (solarYear <= death.Year) return new(true, occurrences, null);
        foreach (int targetYear in new[] { solarYear - 1, solarYear })
        {
            if (targetYear <= original.Year) continue;
            // Giỗ người mất tháng nhuận được cúng vào tháng thường cùng số.
            bool adjustedLeap = original.IsLeapMonth;
            // Ngày 30 của tháng thiếu được lùi về ngày 29.
            bool adjustedDay;
            LunarDate observed;
            DateOnly solar;
            try
            {
                adjustedDay = original.Day == 30 && lunarConverter.GetLunarMonthDays(targetYear, original.Month) == 29;
                observed = new LunarDate(adjustedDay ? 29 : original.Day, original.Month, targetYear, false);
                solar = lunarConverter.LunarToSolar(observed);
            }
            catch (DomainValidationException ex) when (ex.Code == "YEAR_OUT_OF_RANGE") { continue; }
            if (solar.Year == solarYear && solar > death)
                occurrences.Add(new(person.Id, person.FullName, person.Generation, person.BranchId, original, observed, solar,
                    targetYear - original.Year, adjustedLeap, adjustedDay));
        }
        return new(true, occurrences.OrderBy(x => x.SolarDate).ToArray(), null);
    }
}
