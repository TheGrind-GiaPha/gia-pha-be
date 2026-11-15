namespace GiaPha.BusinessObjects.Exceptions;

/// <summary>Mã lỗi thống nhất cho toàn bộ API.</summary>
public static class ApiErrorCodes
{
    public const string Conflict = "CONFLICT";
    public const string NotFound = "NOT_FOUND";
    public const string Validation = "VALIDATION_ERROR";
    public const string RequiresChangeRequest = "REQUIRES_CHANGE_REQUEST";
    public const string DbUpdate = "DB_UPDATE_ERROR";
    public const string Internal = "INTERNAL_ERROR";
}
