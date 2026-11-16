using System.Globalization;
using System.Text;
namespace GiaPha.Services.Helpers;
public static class VietnameseTextNormalizer
{
 public static string Normalize(string? text)
 {
 if(string.IsNullOrWhiteSpace(text)) return "";
 var chars=text.Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark).Select(c=>c is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(c));
 return string.Join(" ", new string(chars.ToArray()).Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
 }
}
