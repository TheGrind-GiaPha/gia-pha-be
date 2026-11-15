using GiaPha.Services.Implementations;
using Xunit;

namespace GiaPha.UnitTests.Services;

public sealed class WordImportServiceTests
{
    private readonly WordImportService _wordService = new();

    [Fact]
    public void ExtractSegments_WithUserSampleV2_ShouldExtractTableContentAndPersons()
    {
        var path = @"D:\Download\Gia_Pha_Ho_Nguyen_Huu_Mau-v2.docx";
        if (!File.Exists(path)) return;

        using var stream = File.OpenRead(path);
        var segments = _wordService.ExtractSegments(stream);

        Assert.NotEmpty(segments);
        var allPersons = segments
            .SelectMany(s => GeminiExtractionService.ExtractPersonsHeuristic(s.Title, s.Content, s.GenerationNumber))
            .ToList();

        Assert.True(allPersons.Count >= 13, $"Expected at least 13 persons, but got {allPersons.Count}");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Hữu Phúc");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Hữu An");
    }

    [Fact]
    public void ExtractSegments_WithUserSampleV1_ShouldExtractGenerationsAndPersons()
    {
        var path = @"D:\Download\Gia_Pha_Ho_Nguyen_Huu_Mau.docx";
        if (!File.Exists(path)) return;

        using var stream = File.OpenRead(path);
        var segments = _wordService.ExtractSegments(stream);

        Assert.NotEmpty(segments);
        Assert.Contains(segments, s => s.GenerationNumber == 1);
        Assert.Contains(segments, s => s.GenerationNumber == 2);
        Assert.Contains(segments, s => s.GenerationNumber == 3);
        Assert.Contains(segments, s => s.GenerationNumber == 4);

        var allPersons = segments
            .SelectMany(s => GeminiExtractionService.ExtractPersonsHeuristic(s.Title, s.Content, s.GenerationNumber))
            .ToList();

        Assert.True(allPersons.Count >= 13, $"Expected at least 13 persons, but got {allPersons.Count}");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Hữu Phúc");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Hữu An");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Hữu Bình");
        Assert.Contains(allPersons, p => p.FullName == "Nguyễn Thị Cúc");
    }
}
