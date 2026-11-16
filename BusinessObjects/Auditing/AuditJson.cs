using System.Text.Json;
using System.Text.Json.Serialization;

namespace GiaPha.BusinessObjects.Auditing;

/// <summary>Cấu hình JSON dùng chung cho audit để Before/After luôn cùng format.</summary>
public static class AuditJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
