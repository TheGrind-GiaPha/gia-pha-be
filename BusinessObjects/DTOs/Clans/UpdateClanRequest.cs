using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Clans;

public sealed class UpdateClanRequest
{
    [Required(ErrorMessage = "Tên dòng họ là bắt buộc.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên dòng họ phải từ 2 đến 200 ký tự.")]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; init; }
}
