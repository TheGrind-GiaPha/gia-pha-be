using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed record FlexibleDateDto(
    int? Year,
    int? Month,
    int? Day,
    CalendarType Calendar,
    bool IsLeapMonth);
