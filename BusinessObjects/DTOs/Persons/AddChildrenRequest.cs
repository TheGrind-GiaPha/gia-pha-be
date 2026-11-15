using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed class AddChildrenRequest
{
    [Required(ErrorMessage = "Danh sách con là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất một người con.")]
    public List<AddChildItem> Children { get; init; } = new();
}
