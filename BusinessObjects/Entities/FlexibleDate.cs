namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Ngày linh hoạt cho ngày sinh / ngày mất trong gia phả.
/// Cho phép thiếu ngày hoặc thiếu tháng, và phân biệt âm lịch / dương lịch.
/// Là owned type của Person (không phải một bảng riêng).
/// </summary>
public sealed class FlexibleDate
{
    public int? Year { get; set; }

    public int? Month { get; set; }

    public int? Day { get; set; }

    public CalendarType Calendar { get; set; } = CalendarType.Solar;

    public bool IsLeapMonth { get; set; }
}
