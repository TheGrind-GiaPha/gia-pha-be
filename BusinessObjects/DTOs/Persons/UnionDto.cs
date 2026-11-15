namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed record UnionDto(
    Guid Id,
    Guid HusbandId,
    Guid WifeId,
    int Rank);
