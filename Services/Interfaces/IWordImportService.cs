namespace GiaPha.Services.Interfaces;

public sealed class WordGenerationSegment
{
    public int GenerationNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Paragraphs { get; set; } = new();
    public List<List<string>> TableRows { get; set; } = new();
}

public interface IWordImportService
{
    /// <summary>
    /// Đọc tài liệu Word .docx và bóc tách thành các đoạn phân theo từng đời ("Đời thứ N").
    /// </summary>
    IReadOnlyList<WordGenerationSegment> ExtractSegments(Stream stream);
}
