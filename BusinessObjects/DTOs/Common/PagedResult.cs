namespace GiaPha.BusinessObjects.DTOs.Common;

/// <summary>Kết quả phân trang dùng chung cho các API danh sách.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total)
{
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(Total / (double)PageSize);
}
