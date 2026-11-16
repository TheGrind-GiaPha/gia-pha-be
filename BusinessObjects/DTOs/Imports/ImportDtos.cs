using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Imports;

/// <summary>
/// Đại diện cho 1 dòng trích xuất từ file Excel theo template chuẩn 10 cột.
/// </summary>
public sealed class ExcelRowData
{
    public string MaDong { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public string? NgaySinh { get; set; }
    public string? LoaiLich { get; set; }
    public string? NgayMat { get; set; }
    public string? MaCha { get; set; }
    public string? MaMe { get; set; }
    public string? LoaiCon { get; set; }
    public int? ThuTuVo { get; set; }
}

/// <summary>
/// Đại diện cho 1 đối tượng được trích xuất từ LLM (Word).
/// </summary>
public sealed class LlmPersonExtracted
{
    public string TempId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";
    public LlmDateExtracted? Birth { get; set; }
    public LlmDateExtracted? Death { get; set; }
    public string? FatherTempId { get; set; }
    public List<LlmSpouseExtracted> Spouses { get; set; } = new();
}

public sealed class LlmDateExtracted
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public string? Calendar { get; set; } = "Solar";
}

public sealed class LlmSpouseExtracted
{
    public string FullName { get; set; } = string.Empty;
    public int Rank { get; set; } = 1;
}

public sealed class ImportRowValidationError
{
    public int RowIndex { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public sealed class ImportRowDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public int RowIndex { get; set; }
    public string ExtractedJson { get; set; } = "{}";
    public ImportRowStatus Status { get; set; }
    public Guid? MatchedPersonId { get; set; }
    public string? MatchedPersonName { get; set; }
    public string? Warnings { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ImportBatchDto
{
    public Guid Id { get; set; }
    public Guid ClanId { get; set; }
    public ImportSource Source { get; set; }
    public ImportBatchStatus Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int AcceptedRowsCount { get; set; }
    public bool ConsentGiven { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<ImportRowDto> Rows { get; set; } = Array.Empty<ImportRowDto>();
}

public sealed class CommitImportBatchRequest
{
    public IReadOnlyList<Guid> AcceptedRowIds { get; set; } = Array.Empty<Guid>();
}

public sealed class CommitImportResultDto
{
    public bool IsPendingApproval { get; set; }
    public Guid? ChangeRequestId { get; set; }
    public int CreatedPersonsCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
