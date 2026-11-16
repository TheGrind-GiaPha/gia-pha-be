namespace GiaPha.BusinessObjects.Common;

public readonly record struct LunarDate(int Day, int Month, int Year, bool IsLeapMonth)
{
    public override string ToString() => IsLeapMonth
        ? $"{Day:D2}/{Month:D2} nhuận/{Year} (ÂL)"
        : $"{Day:D2}/{Month:D2}/{Year} (ÂL)";
}
