/*
 * Copyright (c) 2006 Ho Ngoc Duc. All Rights Reserved.
 * Astronomical algorithms from Jean Meeus, Astronomical Algorithms (1998).
 * https://www.informatik.uni-leipzig.de/~duc/amlich/ (amlich-aa98.js)
 * Permission to use, copy, modify, and redistribute this software and its
 * documentation for personal, non-commercial use is hereby granted provided that
 * this copyright notice and appropriate documentation appears in all copies.
 */
using GiaPha.BusinessObjects.Common;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class LunarConverter : ILunarConverter
{
    public const int MinSupportedYear = 1800;
    public const int MaxSupportedYear = 2199;
    private const double Zone = 7.0;
    private const double Synodic = 29.530588853;
    private const double Epoch = 2415021.076998695;
    private static readonly DateOnly MinDate = new(1800, 1, 1);
    private static readonly DateOnly MaxDate = new(2199, 12, 31);
    private static int Floor(double n) => (int)Math.Floor(n);
    private static DomainValidationException Error(string code, string message) => new(code, message);
    private static void ValidateYear(int year)
    {
        // Lunar 1799 can contain dates in January 1800.
        if (year < 1799 || year > MaxSupportedYear)
            throw Error("YEAR_OUT_OF_RANGE", "Ngày nằm ngoài khoảng hỗ trợ 1800–2199.");
    }
    private static void ValidateMonth(int month)
    {
        if (month is < 1 or > 12) throw Error("INVALID_LUNAR_MONTH", "Tháng âm lịch phải từ 1 đến 12.");
    }
    private static int JdFromDate(int day, int month, int year)
    {
        int a = Floor((14 - month) / 12.0), y = year + 4800 - a, m = month + 12 * a - 3;
        return day + Floor((153 * m + 2) / 5.0) + 365 * y + Floor(y / 4.0) - Floor(y / 100.0) + Floor(y / 400.0) - 32045;
    }
    private static DateOnly JdToDate(int jd)
    {
        int a = jd + 32044, b = Floor((4.0 * a + 3) / 146097), c = a - Floor(b * 146097.0 / 4);
        int d = Floor((4.0 * c + 3) / 1461), e = c - Floor(1461.0 * d / 4), m = Floor((5.0 * e + 2) / 153);
        return new DateOnly(b * 100 + d - 4800 + Floor(m / 10.0), m + 3 - 12 * Floor(m / 10.0), e - Floor((153.0 * m + 2) / 5) + 1);
    }
    private static double NewMoon(int k)
    {
        double t = k / 1236.85, t2 = t * t, t3 = t2 * t, r = Math.PI / 180;
        double jd = 2415020.75933 + 29.53058868 * k + 0.0001178 * t2 - 0.000000155 * t3 + 0.00033 * Math.Sin((166.56 + 132.87 * t - 0.009173 * t2) * r);
        double m = 359.2242 + 29.10535608 * k - 0.0000333 * t2 - 0.00000347 * t3;
        double mp = 306.0253 + 385.81691806 * k + 0.0107306 * t2 + 0.00001236 * t3;
        double f = 21.2964 + 390.67050646 * k - 0.0016528 * t2 - 0.00000239 * t3;
        double c = (0.1734 - 0.000393 * t) * Math.Sin(m * r) + 0.0021 * Math.Sin(2 * m * r)
            - 0.4068 * Math.Sin(mp * r) + 0.0161 * Math.Sin(2 * mp * r) - 0.0004 * Math.Sin(3 * mp * r)
            + 0.0104 * Math.Sin(2 * f * r) - 0.0051 * Math.Sin((m + mp) * r) - 0.0074 * Math.Sin((m - mp) * r)
            + 0.0004 * Math.Sin((2 * f + m) * r) - 0.0004 * Math.Sin((2 * f - m) * r)
            - 0.0006 * Math.Sin((2 * f + mp) * r) + 0.0010 * Math.Sin((2 * f - mp) * r) + 0.0005 * Math.Sin((2 * mp + m) * r);
        double delta = t < -11 ? 0.001 + 0.000839 * t + 0.0002261 * t2 - 0.00000845 * t3 - 0.000000081 * t * t3 : -0.000278 + 0.000265 * t + 0.000262 * t2;
        return jd + c - delta;
    }
    private static int NewMoonDay(int k) => Floor(NewMoon(k) + 0.5 + Zone / 24);
    private static int SunLongitude(int day)
    {
        double t = (day - 0.5 - Zone / 24 - 2451545.0) / 36525, t2 = t * t, r = Math.PI / 180;
        double m = 357.52910 + 35999.05030 * t - 0.0001559 * t2 - 0.00000048 * t * t2;
        double l = 280.46645 + 36000.76983 * t + 0.0003032 * t2
            + (1.914600 - 0.004817 * t - 0.000014 * t2) * Math.Sin(r * m)
            + (0.019993 - 0.000101 * t) * Math.Sin(2 * r * m) + 0.000290 * Math.Sin(3 * r * m);
        l *= r;
        l -= 2 * Math.PI * Floor(l / (2 * Math.PI));
        return Floor(l / Math.PI * 6);
    }
    private static int Month11(int year)
    {
        int k = Floor((JdFromDate(31, 12, year) - 2415021) / Synodic), nm = NewMoonDay(k);
        return SunLongitude(nm) >= 9 ? NewMoonDay(k - 1) : nm;
    }
    private static int LeapOffset(int a11)
    {
        int k = Floor((a11 - Epoch) / Synodic + 0.5), i = 1, arc = SunLongitude(NewMoonDay(k + 1)), last;
        do { last = arc; arc = SunLongitude(NewMoonDay(k + ++i)); } while (arc != last && i < 14);
        return i - 1;
    }
    public LunarDate SolarToLunar(DateOnly date)
    {
        if (date < MinDate || date > MaxDate) throw Error("YEAR_OUT_OF_RANGE", "Ngày dương lịch nằm ngoài khoảng hỗ trợ.");
        int day = JdFromDate(date.Day, date.Month, date.Year), k = Floor((day - Epoch) / Synodic);
        int start = NewMoonDay(k);
        while (start > day) start = NewMoonDay(--k);
        while (NewMoonDay(k + 1) <= day) start = NewMoonDay(++k);
        int a = Month11(date.Year), b = a, year;
        if (a >= start) { year = date.Year; a = Month11(date.Year - 1); }
        else { year = date.Year + 1; b = Month11(date.Year + 1); }
        int diff = Floor((start - a) / 29.0), month = diff + 11; bool leap = false;
        if (b - a > 365) { int leapDiff = LeapOffset(a); if (diff >= leapDiff) { month = diff + 10; leap = diff == leapDiff; } }
        if (month > 12) month -= 12;
        if (month >= 11 && diff < 4) year--;
        return new LunarDate(day - start + 1, month, year, leap);
    }
    private static int MonthStart(int year, int month, bool leap)
    {
        int a = Month11(month < 11 ? year - 1 : year), b = Month11(month < 11 ? year : year + 1);
        int k = Floor(0.5 + (a - Epoch) / Synodic), offset = month - 11;
        if (offset < 0) offset += 12;
        if (b - a > 365)
        {
            int leapOffset = LeapOffset(a), leapMonth = (leapOffset + 10) % 12;
            if (leapMonth == 0) leapMonth = 12;
            if (leap && month != leapMonth) throw Error("INVALID_LEAP_MONTH", $"Năm {year} không có tháng {month} nhuận.");
            if (leap || offset >= leapOffset) offset++;
        }
        else if (leap) throw Error("INVALID_LEAP_MONTH", $"Năm {year} không có tháng {month} nhuận.");
        return NewMoonDay(k + offset);
    }
    public DateOnly LunarToSolar(LunarDate date) => LunarToSolar(date.Day, date.Month, date.Year, date.IsLeapMonth);
    public DateOnly LunarToSolar(int day, int month, int year, bool isLeapMonth = false)
    {
        ValidateYear(year); ValidateMonth(month);
        if (day is < 1 or > 30) throw Error("INVALID_LUNAR_DAY", "Ngày âm lịch phải từ 1 đến 30.");
        int start = MonthStart(year, month, isLeapMonth);
        int k = Floor(0.5 + (start - Epoch) / Synodic);
        if (day > NewMoonDay(k + 1) - start) throw Error("INVALID_LUNAR_DAY", $"Tháng {month}{(isLeapMonth ? " nhuận" : "")} năm {year} chỉ có 29 ngày.");
        DateOnly result = JdToDate(start + day - 1);
        if (result < MinDate || result > MaxDate) throw Error("YEAR_OUT_OF_RANGE", "Ngày dương lịch nằm ngoài khoảng hỗ trợ.");
        return result;
    }
    public int GetLunarMonthDays(int year, int month, bool isLeapMonth = false)
    {
        ValidateYear(year); ValidateMonth(month);
        int start = MonthStart(year, month, isLeapMonth), k = Floor(0.5 + (start - Epoch) / Synodic);
        if (JdToDate(start) > MaxDate || JdToDate(NewMoonDay(k + 1) - 1) < MinDate) throw Error("YEAR_OUT_OF_RANGE", "Tháng nằm ngoài khoảng hỗ trợ.");
        return NewMoonDay(k + 1) - start;
    }
    public int? GetLeapMonth(int year)
    {
        ValidateYear(year);
        for (int month = 1; month <= 12; month++)
        {
            try { MonthStart(year, month, true); return month; }
            catch (DomainValidationException ex) when (ex.Code == "INVALID_LEAP_MONTH") { }
        }
        return null;
    }
    public string FormatLunarDate(FlexibleDate date)
    {
        if (date is null) return string.Empty;
        var parts = new List<string>();
        if (date.Day.HasValue) parts.Add($"{date.Day:D2}");
        if (date.Month.HasValue) parts.Add($"{date.Month:D2}" + (date.Calendar == CalendarType.Lunar && date.IsLeapMonth ? " nhuận" : ""));
        if (date.Year.HasValue) parts.Add(date.Year.Value.ToString());
        if (parts.Count == 0) return string.Empty;
        string original = string.Join("/", parts) + (date.Calendar == CalendarType.Lunar ? " (ÂL)" : " (DL)");
        if (!date.Day.HasValue || !date.Month.HasValue || !date.Year.HasValue) return original;
        try
        {
            if (date.Calendar == CalendarType.Solar) return SolarToLunar(new DateOnly(date.Year.Value, date.Month.Value, date.Day.Value)).ToString();
            var lunar = new LunarDate(date.Day.Value, date.Month.Value, date.Year.Value, date.IsLeapMonth);
            LunarToSolar(lunar);
            return lunar.ToString();
        }
        catch (ArgumentException) { return original; }
        catch (DomainValidationException) { return original; }
    }
}
