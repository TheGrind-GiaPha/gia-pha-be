using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed partial class WordImportService : IWordImportService
{
    private static readonly Regex GenerationHeaderRegex = new(
        @"^(?:ĐỜI|ĐỜI\s+THỨ|THẾ\s+HỆ\s+THỨ|THẾ\s+HỆ|CHI\s+THỨ|PHÂN\s+CHI)\s+([0-9IVXLCDM]+|NHẤT|HAI|BA|BỐN|TƯ|NĂM|SÁU|BẢY|TÁM|CHÍN|MƯỜI|MƯỜI\s+MỘT|MƯỜI\s+HAI)[\s:.\-—]*(.*)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<WordGenerationSegment> ExtractSegments(Stream stream)
    {
        var segments = new List<WordGenerationSegment>();
        WordGenerationSegment? currentSegment = null;

        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return segments;

        foreach (var element in body.Elements())
        {
            if (element is Paragraph p)
            {
                var text = p.InnerText.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                var match = GenerationHeaderRegex.Match(text);
                if (match.Success)
                {
                    // Gặp đời mới
                    var numStr = match.Groups[1].Value;
                    var genNumber = ParseRomanOrArabic(numStr);

                    currentSegment = new WordGenerationSegment
                    {
                        GenerationNumber = genNumber,
                        Title = text
                    };
                    segments.Add(currentSegment);
                    continue;
                }

                if (currentSegment is null)
                {
                    // Nếu chưa gặp "Đời thứ N", có thể là phần Lời tựa/Mở đầu (Đời 0)
                    currentSegment = new WordGenerationSegment
                    {
                        GenerationNumber = 0,
                        Title = "Phần mở đầu / Khởi tổ"
                    };
                    segments.Add(currentSegment);
                }

                currentSegment.Paragraphs.Add(text);
            }
            else if (element is Table table)
            {
                if (currentSegment is null)
                {
                    currentSegment = new WordGenerationSegment
                    {
                        GenerationNumber = 0,
                        Title = "Bảng dữ liệu ban đầu"
                    };
                    segments.Add(currentSegment);
                }

                foreach (var row in table.Elements<TableRow>())
                {
                    var cellValues = row.Elements<TableCell>()
                        .Select(c => c.InnerText.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();

                    if (cellValues.Count > 0)
                    {
                        currentSegment.TableRows.Add(cellValues);
                        currentSegment.Paragraphs.Add(string.Join(" | ", cellValues));
                    }
                }
            }
        }

        // Tạo nội dung Content hoàn chỉnh cho từng segment
        foreach (var seg in segments)
        {
            seg.Content = string.Join("\n", seg.Paragraphs);
        }

        return segments;
    }

    private static int ParseRomanOrArabic(string input)
    {
        if (int.TryParse(input, out var arabic)) return arabic;

        var clean = input.Trim().ToUpperInvariant();
        return clean switch
        {
            "NHẤT" or "MOT" or "MỘT" or "I" => 1,
            "HAI" or "II" => 2,
            "BA" or "III" => 3,
            "BỐN" or "BON" or "TƯ" or "TU" or "IV" => 4,
            "NĂM" or "NAM" or "V" => 5,
            "SÁU" or "SAU" or "VI" => 6,
            "BẢY" or "BAY" or "VII" => 7,
            "TÁM" or "TAM" or "VIII" => 8,
            "CHÍN" or "CHIN" or "IX" => 9,
            "MƯỜI" or "MUOI" or "X" => 10,
            _ => ParseRoman(clean)
        };
    }

    private static int ParseRoman(string roman)
    {
        var romanMap = new Dictionary<char, int>
        {
            {'I', 1}, {'V', 5}, {'X', 10}, {'L', 50}, {'C', 100}, {'D', 500}, {'M', 1000}
        };

        var total = 0;
        var prevValue = 0;

        foreach (var c in roman.Reverse())
        {
            if (romanMap.TryGetValue(c, out var val))
            {
                if (val < prevValue)
                    total -= val;
                else
                    total += val;
                prevValue = val;
            }
        }

        return total > 0 ? total : 1;
    }
}
