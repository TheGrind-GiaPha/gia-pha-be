using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

public sealed record FlexibleDateDto(
    int? Year = null,
    int? Month = null,
    int? Day = null,
    CalendarType Calendar = CalendarType.Solar,
    bool IsLeapMonth = false);
