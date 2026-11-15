using System.Globalization;
using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Auditing;

/// <summary>
/// Chuyển JSON Before/After thành các dòng diff tiếng Việt cho người dùng cuối.
/// Không trả tên field kỹ thuật nếu đã có nhãn tương ứng.
/// </summary>
public static class AuditDiffFormatter
{
    private static readonly HashSet<string> IgnoredFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "clanId",
        "createdAt",
        "updatedAt",
        "path",
        "version"
    };

    private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = "Tên dòng họ",
        ["description"] = "Mô tả",
        ["originStory"] = "Phả ký",
        ["rules"] = "Quy ước",
        ["status"] = "Trạng thái",
        ["publishedAt"] = "Ngày công bố",
        ["fullName"] = "Họ tên",
        ["huyName"] = "Tên húy",
        ["tuName"] = "Tên tự",
        ["thuyName"] = "Tên thụy",
        ["gender"] = "Giới tính",
        ["birthDate"] = "Ngày sinh",
        ["deathDate"] = "Ngày mất",
        ["isLiving"] = "Còn sống",
        ["generation"] = "Đời",
        ["privacyLevel"] = "Mức riêng tư",
        ["portraitUrl"] = "Ảnh chân dung",
        ["mergedIntoId"] = "Gộp vào",
        ["branchId"] = "Chi",
        ["founderPersonId"] = "Người sáng lập chi",
        ["siblingOrder"] = "Thứ tự anh em",
        ["relationType"] = "Loại quan hệ",
        ["rank"] = "Thứ tự vợ",
        ["husbandId"] = "Chồng",
        ["wifeId"] = "Vợ",
        ["parentId"] = "Cha/mẹ",
        ["childId"] = "Con"
    };

    private static readonly Dictionary<string, int> FieldOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fullName"] = 0,
        ["name"] = 0,
        ["huyName"] = 1,
        ["tuName"] = 2,
        ["thuyName"] = 3,
        ["gender"] = 4,
        ["birthDate"] = 5,
        ["deathDate"] = 6,
        ["isLiving"] = 7,
        ["generation"] = 8,
        ["description"] = 9,
        ["originStory"] = 10,
        ["rules"] = 11,
        ["status"] = 12,
        ["publishedAt"] = 13,
        ["privacyLevel"] = 14,
        ["portraitUrl"] = 15,
        ["mergedIntoId"] = 16,
        ["branchId"] = 17
    };

    public static AuditLogDto ToDto(AuditLog log)
    {
        var before = Parse(log.Before);
        var after = Parse(log.After);
        var occurredAt = log.OccurredAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(log.OccurredAt, DateTimeKind.Utc)
            : log.OccurredAt.ToUniversalTime();

        return new AuditLogDto(
            log.Id,
            log.ClanId,
            log.EntityType,
            log.EntityId,
            log.Action.ToString(),
            ActionLabel(log.Action),
            log.ActorId,
            log.ActorId is null ? "Hệ thống" : $"Người dùng {log.ActorId.Value.ToString()[..8]}",
            log.ChangeRequestId,
            log.BatchId,
            BuildSummary(log, before, after),
            BuildChanges(log.EntityType, before, after),
            occurredAt);
    }

    private static IReadOnlyList<AuditChangeDto> BuildChanges(
        string entityType,
        IReadOnlyDictionary<string, JsonElement> before,
        IReadOnlyDictionary<string, JsonElement> after)
    {
        var keys = before.Keys
            .Concat(after.Keys)
            .Where(key => !IgnoredFields.Contains(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => FieldOrder.TryGetValue(key, out var order) ? order : 999)
            .ThenBy(FieldLabel, StringComparer.CurrentCulture)
            .ToList();

        var changes = new List<AuditChangeDto>(keys.Count);

        foreach (var key in keys)
        {
            var hasBefore = before.TryGetValue(key, out var beforeValue);
            var hasAfter = after.TryGetValue(key, out var afterValue);

            var beforeText = hasBefore ? FormatValue(entityType, key, beforeValue) : "(trống)";
            var afterText = hasAfter ? FormatValue(entityType, key, afterValue) : "(trống)";

            if (string.Equals(beforeText, afterText, StringComparison.Ordinal))
            {
                continue;
            }

            changes.Add(new AuditChangeDto(
                key,
                FieldLabel(key),
                beforeText,
                afterText));
        }

        return changes;
    }

    private static Dictionary<string, JsonElement> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            return document.RootElement
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => property.Value.Clone(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string BuildSummary(
        AuditLog log,
        IReadOnlyDictionary<string, JsonElement> before,
        IReadOnlyDictionary<string, JsonElement> after)
    {
        var name = GetString(after, "fullName")
            ?? GetString(after, "name")
            ?? GetString(before, "fullName")
            ?? GetString(before, "name");

        var entity = EntityLabel(log.EntityType);

        return name is null
            ? $"{ActionLabel(log.Action)} {entity}"
            : $"{ActionLabel(log.Action)} {entity} {name}";
    }

    private static string? GetString(IReadOnlyDictionary<string, JsonElement> values, string key)
        => values.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string FieldLabel(string key)
        => FieldLabels.TryGetValue(key, out var label) ? label : key;

    private static string ActionLabel(AuditAction action) => action switch
    {
        AuditAction.Create => "Tạo mới",
        AuditAction.Update => "Cập nhật",
        AuditAction.Delete => "Xóa",
        AuditAction.Restore => "Khôi phục",
        _ => action.ToString()
    };

    private static string EntityLabel(string entityType) => entityType switch
    {
        "Person" => "thành viên",
        "Clan" => "dòng họ",
        "Branch" => "chi",
        _ => entityType
    };

    private static string FormatValue(string entityType, string key, JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return "(trống)";
        }

        if (key.Equals("birthDate", StringComparison.OrdinalIgnoreCase)
            || key.Equals("deathDate", StringComparison.OrdinalIgnoreCase))
        {
            return FormatFlexibleDate(value);
        }

        if (key.Equals("publishedAt", StringComparison.OrdinalIgnoreCase))
        {
            return FormatDateTime(value);
        }

        if (key.Equals("gender", StringComparison.OrdinalIgnoreCase))
        {
            return FormatGender(value);
        }

        if (key.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            return FormatStatus(entityType, value);
        }

        if (key.Equals("privacyLevel", StringComparison.OrdinalIgnoreCase))
        {
            return FormatPrivacy(value);
        }

        if (key.Equals("relationType", StringComparison.OrdinalIgnoreCase))
        {
            return FormatRelation(value);
        }

        if (key.Equals("isLiving", StringComparison.OrdinalIgnoreCase))
        {
            return value.ValueKind == JsonValueKind.True ? "Có" : "Không";
        }

        if (key.Equals("portraitUrl", StringComparison.OrdinalIgnoreCase)
            && value.ValueKind == JsonValueKind.String)
        {
            var url = value.GetString();
            if (string.IsNullOrWhiteSpace(url))
            {
                return "(trống)";
            }

            return Path.GetFileName(url);
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "(trống)",
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "Có",
            JsonValueKind.False => "Không",
            JsonValueKind.Object => value.GetRawText(),
            JsonValueKind.Array => string.Join(", ", value.EnumerateArray().Select(FormatScalar)),
            _ => value.GetRawText()
        };
    }

    private static string FormatScalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "(trống)",
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "Có",
        JsonValueKind.False => "Không",
        JsonValueKind.Null => "(trống)",
        _ => value.GetRawText()
    };

    private static string FormatDateTime(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            return value.GetRawText();
        }

        return DateTime.TryParse(
            value.GetString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
            : value.GetString() ?? "(trống)";
    }

    private static string FormatFlexibleDate(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            return FormatScalar(value);
        }

        var year = GetInt(value, "year");
        var month = GetInt(value, "month");
        var day = GetInt(value, "day");
        var calendar = GetString(value, "calendar");

        if (year is null)
        {
            return "(trống)";
        }

        var text = month is null
            ? year.Value.ToString(CultureInfo.InvariantCulture)
            : day is null
                ? $"{month.Value:00}/{year.Value}"
                : $"{day.Value:00}/{month.Value:00}/{year.Value}";

        if (string.Equals(calendar, "Lunar", StringComparison.OrdinalIgnoreCase)
            || calendar == "1")
        {
            text += " (âm lịch)";
        }

        return text;
    }

    private static int? GetInt(JsonElement value, string propertyName)
    {
        if (!value.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Number
            || !property.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    private static string? GetString(JsonElement value, string propertyName)
        => value.TryGetProperty(propertyName, out var property)
           && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string FormatGender(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() switch
        {
            "Male" => "Nam",
            "Female" => "Nữ",
            "Other" => "Khác",
            _ => value.GetString() ?? "(trống)"
        },
        JsonValueKind.Number when value.TryGetInt32(out var number) => number switch
        {
            0 => "Nam",
            1 => "Nữ",
            2 => "Khác",
            _ => value.GetRawText()
        },
        _ => value.GetRawText()
    };

    private static string FormatStatus(string entityType, JsonElement value)
    {
        var status = value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
                ? number.ToString(CultureInfo.InvariantCulture)
                : null;

        if (status is null)
        {
            return value.GetRawText();
        }

        return entityType switch
        {
            "Clan" => status switch
            {
                "Draft" or "0" => "Bản nháp",
                "Published" or "1" => "Đã công bố",
                _ => status
            },
            "Person" => status switch
            {
                "Active" or "0" => "Đang hoạt động",
                "Deleted" or "1" => "Đã xóa",
                _ => status
            },
            _ => status
        };
    }

    private static string FormatPrivacy(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() switch
        {
            "Public" => "Công khai",
            "Family" => "Trong họ",
            "Private" => "Riêng tư",
            _ => value.GetString() ?? "(trống)"
        },
        JsonValueKind.Number when value.TryGetInt32(out var number) => number switch
        {
            0 => "Công khai",
            1 => "Trong họ",
            2 => "Riêng tư",
            _ => value.GetRawText()
        },
        _ => value.GetRawText()
    };

    private static string FormatRelation(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() switch
        {
            "Biological" => "Con ruột",
            "Adopted" => "Con nuôi",
            "Heir" => "Con kế tự",
            _ => value.GetString() ?? "(trống)"
        },
        JsonValueKind.Number when value.TryGetInt32(out var number) => number switch
        {
            0 => "Con ruột",
            1 => "Con nuôi",
            2 => "Con kế tự",
            _ => value.GetRawText()
        },
        _ => value.GetRawText()
    };
}
