using System.ComponentModel.DataAnnotations;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed class AddChildItem
{
    [Required(ErrorMessage = "Họ tên con là bắt buộc.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên con phải từ 2 đến 200 ký tự.")]
    public string FullName { get; init; } = string.Empty;

    public Gender Gender { get; init; }

    public FlexibleDateDto? BirthDate { get; init; }

    public RelationType RelationType { get; init; } = RelationType.Biological;
}
