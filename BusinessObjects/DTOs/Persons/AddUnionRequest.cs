using System.ComponentModel.DataAnnotations;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed class AddUnionRequest
{
    [Required(ErrorMessage = "Người vợ là bắt buộc.")]
    public Guid WifeId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Rank phải lớn hơn 0.")]
    public int Rank { get; init; } = 1;
}
