namespace GiaPha.BusinessObjects.Entities;

/// <summary>Giới tính của một người trong gia phả.</summary>
public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

/// <summary>Trạng thái vòng đời của một Person. Không bao giờ xóa cứng.</summary>
public enum PersonStatus
{
    Active = 0,
    Deleted = 1
}

/// <summary>Mức độ công khai thông tin (đặc biệt với người còn sống).</summary>
public enum PrivacyLevel
{
    Public = 0,
    Family = 1,
    Private = 2
}

/// <summary>Loại quan hệ cha - con.</summary>
public enum RelationType
{
    Biological = 0,
    Adopted = 1,
    Heir = 2
}

/// <summary>Loại lịch dùng cho ngày sinh / ngày mất.</summary>
public enum CalendarType
{
    Solar = 0,
    Lunar = 1
}

/// <summary>Trạng thái của một dòng họ (Clan).</summary>
public enum ClanStatus
{
    Draft = 0,
    Published = 1
}

/// <summary>Chế độ hiển thị của dòng họ: riêng tư hoặc công khai cho khách xem.</summary>
public enum ClanVisibility
{
    Private = 0,
    Public = 1
}
