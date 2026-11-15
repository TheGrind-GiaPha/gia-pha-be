using System.ComponentModel.DataAnnotations;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.BusinessObjects.DTOs.Persons;

/// <summary>Vợ/chồng hiển thị trong hồ sơ (chỉ lấy người kia, kèm Rank).</summary>
public sealed record SpouseDto(
    Guid UnionId,
    Guid PersonId,
    string FullName,
    int Rank);

/// <summary>Hồ sơ đầy đủ một người: thông tin cá nhân + vợ/chồng + cha/mẹ + con.</summary>
public sealed record PersonProfileDto(
    Guid Id,
    Guid ClanId,
    string FullName,
    string? HuyName,
    string? TuName,
    string? ThuyName,
    string? OtherNames,
    Gender Gender,
    FlexibleDateDto? BirthDate,
    FlexibleDateDto? DeathDate,
    bool IsLiving,
    int Generation,
    Guid? BranchId,
    PrivacyLevel PrivacyLevel,
    string? PortraitUrl,
    string? Biography,
    string? BurialPlace,
    string? BurialMapUrl,
    string? Contact,
    bool HasHiddenChildren,
    IReadOnlyList<SpouseDto> Spouses,
    IReadOnlyList<PersonDto> Parents,
    IReadOnlyList<PersonDto> Children);

/// <summary>Request dùng chung cho "Thêm thủy tổ" và "Sửa người".</summary>
public class PersonProfileRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 200 ký tự.")]
    public string FullName { get; init; } = string.Empty;

    public Gender Gender { get; init; } = Gender.Male;

    [StringLength(200)] public string? HuyName { get; init; }

    [StringLength(200)] public string? TuName { get; init; }

    [StringLength(200)] public string? ThuyName { get; init; }

    [StringLength(500)] public string? OtherNames { get; init; }

    public FlexibleDateDto? BirthDate { get; init; }

    public FlexibleDateDto? DeathDate { get; init; }

    [StringLength(4000)] public string? Biography { get; init; }

    [StringLength(500)] public string? BurialPlace { get; init; }

    [StringLength(500)] public string? BurialMapUrl { get; init; }

    [StringLength(500)] public string? Contact { get; init; }

    public PrivacyLevel PrivacyLevel { get; init; } = PrivacyLevel.Public;
}
