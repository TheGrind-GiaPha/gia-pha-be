using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Changes;

public sealed record AddPersonChangePayload(
    Guid ClanId,
    Guid FatherId,
    string FullName,
    Gender Gender,
    FlexibleDateDto? BirthDate,
    RelationType RelationType);

public sealed record MoveSubtreeChangePayload(
    Guid ClanId,
    Guid PersonId,
    Guid NewFatherId);

public sealed record DeletePersonChangePayload(
    Guid ClanId,
    Guid PersonId,
    string Strategy,
    Guid? NewFatherId);

public sealed record MergePersonsChangePayload(
    Guid ClanId,
    Guid KeepId,
    Guid RemoveId);
