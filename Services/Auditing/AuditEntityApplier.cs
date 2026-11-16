using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Auditing;

/// <summary>
/// Áp dụng Before/After JSON vào entity cho tính năng restore.
/// Chỉ xử lý field dữ liệu; bỏ qua field kỹ thuật/cấu trúc.
/// </summary>
public static class AuditEntityApplier
{
    private static readonly HashSet<string> NonRestorableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "clanId",
        "createdAt",
        "updatedAt",
        "path",
        "generation",
        "version",
        "mergedIntoId"
    };

    private static readonly HashSet<string> StructuralFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "status",
        "mergedIntoId",
        "path",
        "generation"
    };

    public static bool HasStructuralChange(IReadOnlyDictionary<string, JsonElement> after)
        => after.Keys.Any(key => StructuralFields.Contains(key));

    public static bool HasRestorableField(IReadOnlyDictionary<string, JsonElement> before)
        => before.Keys.Any(key => !NonRestorableFields.Contains(key));

    public static IReadOnlyList<string> FindMismatches(
        object entity,
        IReadOnlyDictionary<string, JsonElement> after)
    {
        var properties = BuildPropertyMap(entity.GetType());
        var mismatches = new List<string>();

        foreach (var (key, expected) in after)
        {
            if (NonRestorableFields.Contains(key))
            {
                continue;
            }

            if (!properties.TryGetValue(key, out var property))
            {
                continue;
            }

            var current = property.GetValue(entity);
            var currentJson = JsonSerializer.SerializeToElement(
                current,
                property.PropertyType,
                AuditJson.Options);

            if (!JsonNode.DeepEquals(
                    JsonNode.Parse(currentJson.GetRawText()),
                    JsonNode.Parse(expected.GetRawText())))
            {
                mismatches.Add(AuditDiffFormatter.Label(key));
            }
        }

        return mismatches;
    }

    public static int Apply(object entity, IReadOnlyDictionary<string, JsonElement> before)
    {
        var properties = BuildPropertyMap(entity.GetType());
        var applied = 0;

        foreach (var (key, value) in before)
        {
            if (NonRestorableFields.Contains(key))
            {
                continue;
            }

            if (!properties.TryGetValue(key, out var property) || !property.CanWrite)
            {
                continue;
            }

            var converted = ConvertValue(value, property.PropertyType);
            property.SetValue(entity, converted);
            applied++;
        }

        return applied;
    }

    private static Dictionary<string, PropertyInfo> BuildPropertyMap(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead)
            .ToDictionary(
                property => JsonNamingPolicy.CamelCase.ConvertName(property.Name),
                property => property,
                StringComparer.OrdinalIgnoreCase);

    private static object? ConvertValue(JsonElement value, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;
        }

        if (underlying == typeof(string))
        {
            return value.GetString();
        }

        if (underlying == typeof(Guid))
        {
            return value.GetGuid();
        }

        if (underlying == typeof(int))
        {
            return value.GetInt32();
        }

        if (underlying == typeof(bool))
        {
            return value.GetBoolean();
        }

        if (underlying == typeof(DateTime))
        {
            return value.GetDateTime();
        }

        if (underlying == typeof(FlexibleDate))
        {
            return JsonSerializer.Deserialize<FlexibleDate>(value.GetRawText(), AuditJson.Options);
        }

        if (underlying.IsEnum)
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                return Enum.Parse(underlying, value.GetString()!, ignoreCase: true);
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            {
                return Enum.ToObject(underlying, number);
            }
        }

        return JsonSerializer.Deserialize(value.GetRawText(), targetType, AuditJson.Options);
    }
}
