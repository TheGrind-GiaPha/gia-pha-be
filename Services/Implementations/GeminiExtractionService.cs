using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;
using Microsoft.Extensions.Configuration;

namespace GiaPha.Services.Implementations;

public sealed partial class GeminiExtractionService : ILlmExtractionService
{
    private static readonly Regex PhoneRegex = new(
        @"(?:\+?84|0)(?:3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-9])[0-9]{7}\b",
        RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly ILoggerManager _logger;
    private readonly string _apiKey;
    private readonly string _model;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeminiExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILoggerManager logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public string MaskSensitiveData(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return PhoneRegex.Replace(input, "[ĐÃ CHE SĐT]");
    }

    public async Task<IReadOnlyList<LlmPersonExtracted>> ExtractPersonsAsync(
        string segmentTitle,
        string segmentText,
        int generationNumber,
        CancellationToken cancellationToken = default)
    {
        var maskedText = MaskSensitiveData(segmentText);

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            try
            {
                var systemPrompt = """
                    Bạn là chuyên gia phân tích phả ký dòng họ Việt Nam. 
                    Nhiệm vụ: Trích xuất danh sách nhân khẩu từ đoạn văn bản gia phả được cung cấp.
                    Quy tắc:
                    1. Phải chuẩn hóa họ tên chữ quốc ngữ có dấu.
                    2. Phân tích quan hệ: ai là cha (fatherTempId), ai là con, ai là vợ/chồng (spouses với rank 1, 2...).
                    3. Xác định ngày sinh, ngày mất: năm, tháng, ngày (nếu âm lịch thì ghi calendar: "Lunar", mặc định "Solar").
                    4. Chỉ trả về JSON hợp lệ theo schema:
                    {
                       "persons": [
                          {
                             "tempId": "string",
                             "fullName": "string",
                             "gender": "Male | Female | Other",
                             "birth": { "year": 1950, "month": 5, "day": 10, "calendar": "Solar" },
                             "death": { "year": null, "month": null, "day": null, "calendar": "Solar" },
                             "fatherTempId": "string hoặc null",
                             "spouses": [ { "fullName": "string", "rank": 1 } ]
                          }
                       ]
                    }
                    Không kèm bất kỳ lời giải thích nào khác ngoài chuỗi JSON.
                    """;

                var userPrompt = $"Tiêu đề đoạn: {segmentTitle} (Thế hệ: {generationNumber})\nNội dung văn bản:\n{maskedText}";

                // Lần thử 1
                var jsonResponse = await CallGeminiAsync(systemPrompt, userPrompt, cancellationToken);
                var parsed = TryParsePersons(jsonResponse);

                if (parsed is not null && parsed.Count > 0)
                {
                    return parsed;
                }

                _logger.LogWarn($"Lần 1 trích xuất đoạn '{segmentTitle}' không hợp lệ hoặc rỗng. Đang thử lại lần 2...");

                // Retry lần 1 với prompt sửa lỗi
                var repairPrompt = $"""
                    Kết quả JSON trước của bạn không hợp lệ hoặc rỗng:
                    ---
                    {jsonResponse}
                    ---
                    Vui lòng sửa lại và đảm bảo trả về đúng cú pháp JSON chứa thuộc tính 'persons' từ văn bản sau:
                    {userPrompt}
                    """;

                var retryResponse = await CallGeminiAsync(systemPrompt, repairPrompt, cancellationToken);
                var retryParsed = TryParsePersons(retryResponse);

                if (retryParsed is not null && retryParsed.Count > 0)
                {
                    return retryParsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarn($"Gọi Gemini API không thành công ({ex.Message}). Sử dụng bộ phân tích phả ký nội bộ thay thế.");
            }
        }
        else
        {
            _logger.LogInfo("Chưa cấu hình Gemini:ApiKey. Sử dụng bộ phân tích ngữ nghĩa phả ký dự phòng.");
        }

        // Heuristic extraction fallback
        return ExtractPersonsHeuristic(segmentTitle, maskedText, generationNumber);
    }

    private async Task<string> CallGeminiAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "{}";
        }

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"{systemPrompt}\n\n{userPrompt}" }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0 &&
            candidates[0].TryGetProperty("content", out var content) &&
            content.TryGetProperty("parts", out var parts) &&
            parts.GetArrayLength() > 0 &&
            parts[0].TryGetProperty("text", out var textElem))
        {
            return textElem.GetString() ?? "{}";
        }

        return "{}";
    }

    private static IReadOnlyList<LlmPersonExtracted>? TryParsePersons(string json)
    {
        try
        {
            var cleanedJson = json.Trim();
            if (cleanedJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanedJson = cleanedJson[7..];
            }
            if (cleanedJson.StartsWith("```"))
            {
                cleanedJson = cleanedJson[3..];
            }
            if (cleanedJson.EndsWith("```"))
            {
                cleanedJson = cleanedJson[..^3];
            }
            cleanedJson = cleanedJson.Trim();

            var wrapper = JsonSerializer.Deserialize<LlmResponseWrapper>(cleanedJson, JsonOptions);
            return wrapper?.Persons;
        }
        catch
        {
            return null;
        }
    }

    public static IReadOnlyList<LlmPersonExtracted> ExtractPersonsHeuristic(
        string segmentTitle,
        string segmentText,
        int generationNumber)
    {
        var list = new List<LlmPersonExtracted>();
        if (string.IsNullOrWhiteSpace(segmentText)) return list;

        var lines = segmentText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // TH1: Bảng dữ liệu Word có chứa dấu '|'
        var tableLines = lines.Where(l => l.Contains('|')).ToList();
        if (tableLines.Count >= 2)
        {
            var headerLine = tableLines.FirstOrDefault(l =>
                l.Contains("Họ", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("Tên", StringComparison.OrdinalIgnoreCase));

            if (headerLine is not null)
            {
                var headers = headerLine.Split('|').Select(h => h.Trim().ToLowerInvariant()).ToList();
                var nameCol = headers.FindIndex(h =>
                    !h.Contains("chọn") &&
                    (h.Contains("họ và tên") || h.Contains("họ tên") || h.Contains("hoten") || h.Contains("fullname") || h.Contains("tên") || h.StartsWith("họ")));
                var genderCol = headers.FindIndex(h => h.Contains("giới") || h.Contains("gender") || h.Contains("gioitinh"));
                var birthCol = headers.FindIndex(h => h.Contains("sinh") || h.Contains("birth"));
                var deathCol = headers.FindIndex(h => h.Contains("mất") || h.Contains("death"));
                var fatherCol = headers.FindIndex(h => (h.Contains("cha") || h.Contains("father")) && !h.Contains("thao tác"));
                var codeCol = headers.FindIndex(h => (h.Contains("dòng") || h.Contains("mã") || h.Contains("stt") || h.Contains("id")) && !h.Contains("cha"));

                if (nameCol != -1)
                {
                    var headerIdx = tableLines.IndexOf(headerLine);
                    for (var i = headerIdx + 1; i < tableLines.Count; i++)
                    {
                        var cells = tableLines[i].Split('|').Select(c => c.Trim()).ToList();
                        if (cells.Count <= nameCol) continue;

                        var name = cells[nameCol];
                        if (string.IsNullOrWhiteSpace(name) || name.Contains("Họ và tên", StringComparison.OrdinalIgnoreCase)) continue;

                        var rawGender = genderCol != -1 && cells.Count > genderCol ? cells[genderCol] : "Nam";
                        var gender = rawGender.Contains("nữ", StringComparison.OrdinalIgnoreCase) ? "Female" : "Male";

                        var rawBirth = birthCol != -1 && cells.Count > birthCol ? cells[birthCol] : "";
                        var rawDeath = deathCol != -1 && cells.Count > deathCol ? cells[deathCol] : "";
                        var rawFather = fatherCol != -1 && cells.Count > fatherCol ? cells[fatherCol] : "";
                        var rawCode = codeCol != -1 && cells.Count > codeCol ? cells[codeCol] : (i - headerIdx).ToString();

                        var birthYear = ExtractYear(rawBirth);
                        var deathYear = ExtractYear(rawDeath);
                        var fatherId = CleanCode(rawFather);

                        list.Add(new LlmPersonExtracted
                        {
                            TempId = string.IsNullOrWhiteSpace(rawCode) ? (i - headerIdx).ToString() : rawCode,
                            FullName = name,
                            Gender = gender,
                            Birth = birthYear.HasValue ? new LlmDateExtracted { Year = birthYear.Value, Calendar = "Solar" } : null,
                            Death = deathYear.HasValue ? new LlmDateExtracted { Year = deathYear.Value, Calendar = "Solar" } : null,
                            FatherTempId = fatherId
                        });
                    }

                    if (list.Count > 0) return list;
                }
            }
        }

        // TH2: Đoạn văn bản phả ký qua các đời (ví dụ: 1.1. Cụ ông Nguyễn Hữu Phúc...)
        var personRegex = new Regex(
            @"(?:^|\n)\s*(?:([0-9]+(?:\.[0-9]+)*)\.?\s*)?(?:Thủy\s+tổ:\s*)?(?:Cụ\s+ông|Cụ\s+bà|Ông|Bà|Anh|Chị|Cụ)?\s*([A-ZÀ-Ỹ][a-zà-ỹ]+(?:\s+[A-ZÀ-Ỹ][a-zà-ỹ]+)+)(?:[^\n(]*\(([^)]*)\))?",
            RegexOptions.Compiled);

        var matches = personRegex.Matches(segmentText);
        var autoTempIdx = 1;

        foreach (Match match in matches)
        {
            var tempId = match.Groups[1].Value.Trim();
            var fullName = match.Groups[2].Value.Trim();
            var parens = match.Groups[3].Value.Trim();

            if (string.IsNullOrWhiteSpace(fullName)) continue;

            // Bỏ qua các chuỗi tiêu đề không phải tên người
            if (fullName.StartsWith("Cộng Hòa", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Gia Phả", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Phần ", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Đời Thứ", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Thế Hệ", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Nguyên Quán", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Nơi Định", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Tộc Ước", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(tempId))
            {
                tempId = generationNumber > 0 ? $"{generationNumber}.{autoTempIdx++}" : autoTempIdx++.ToString();
            }

            var matchedPrefix = match.Value.ToLowerInvariant();
            var isFemale = matchedPrefix.Contains("bà") || matchedPrefix.Contains("nữ") || fullName.Contains("Thị");
            var gender = isFemale ? "Female" : "Male";

            var (birthYear, deathYear) = ParseYearsFromParens(parens);

            string? fatherTempId = null;
            if (!string.IsNullOrWhiteSpace(parens))
            {
                var fatherMatch = Regex.Match(parens, @"Con\s+(?:trưởng|thứ|gái|út)?\s*(?:ông|bà|cụ)?\s*([A-ZÀ-Ỹ][a-zà-ỹ]+)", RegexOptions.IgnoreCase);
                if (fatherMatch.Success)
                {
                    var fatherNamePart = fatherMatch.Groups[1].Value.Trim();
                    var parent = list.FirstOrDefault(p => p.FullName.EndsWith(fatherNamePart, StringComparison.OrdinalIgnoreCase));
                    if (parent is not null)
                    {
                        fatherTempId = parent.TempId;
                    }
                }
            }

            // Tìm thông tin vợ/chồng trong đoạn văn bản xung quanh
            var spouses = new List<LlmSpouseExtracted>();
            var spouseRegex = new Regex(@"(?:Chính\s+thất|Kế\s+thất|Thứ\s+thất|Vợ|Chồng):\s*(?:Cụ\s+bà|Cụ\s+ông|Bà|Ông)?\s*([A-ZÀ-Ỹ][a-zà-ỹ]+(?:\s+[A-ZÀ-Ỹ][a-zà-ỹ]+)+)");
            var spouseMatch = spouseRegex.Match(segmentText);
            if (spouseMatch.Success && list.Count == 0) // gán vợ cho người đầu tiên trong đoạn nếu có
            {
                spouses.Add(new LlmSpouseExtracted
                {
                    FullName = spouseMatch.Groups[1].Value.Trim(),
                    Rank = 1
                });
            }

            list.Add(new LlmPersonExtracted
            {
                TempId = tempId,
                FullName = fullName,
                Gender = gender,
                Birth = birthYear.HasValue ? new LlmDateExtracted { Year = birthYear.Value, Calendar = "Solar" } : null,
                Death = deathYear.HasValue ? new LlmDateExtracted { Year = deathYear.Value, Calendar = "Solar" } : null,
                FatherTempId = fatherTempId,
                Spouses = spouses
            });
        }

        // Bổ sung cha cho thế hệ 2 nếu chưa có: con cụ đời 1
        var gen1 = list.FirstOrDefault(p => p.TempId.StartsWith("1.") || p.TempId == "1");
        if (gen1 is not null)
        {
            foreach (var p in list.Where(x => x != gen1 && string.IsNullOrWhiteSpace(x.FatherTempId)))
            {
                if (p.TempId.StartsWith("2."))
                {
                    p.FatherTempId = gen1.TempId;
                }
            }
        }

        return list;
    }

    private static (int? BirthYear, int? DeathYear) ParseYearsFromParens(string parens)
    {
        if (string.IsNullOrWhiteSpace(parens)) return (null, null);

        // Tìm tất cả các số có 4 chữ số (năm)
        var matches = Regex.Matches(parens, @"\b(1[789]\d{2}|20\d{2})\b");
        if (matches.Count >= 2)
        {
            if (int.TryParse(matches[0].Value, out var b) && int.TryParse(matches[1].Value, out var d))
            {
                return (b, d);
            }
        }
        else if (matches.Count == 1)
        {
            if (int.TryParse(matches[0].Value, out var b))
            {
                return (b, null);
            }
        }

        return (null, null);
    }

    private static int? ExtractYear(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var match = Regex.Match(val, @"\b(1[789]\d{2}|20\d{2})\b");
        return match.Success && int.TryParse(match.Value, out var yr) ? yr : null;
    }

    private static string? CleanCode(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var t = val.Trim();
        return t is "-" or "0" or "none" or "null" ? null : t;
    }

    private sealed class LlmResponseWrapper
    {
        public List<LlmPersonExtracted> Persons { get; set; } = new();
    }
}
