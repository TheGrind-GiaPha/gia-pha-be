namespace GiaPha.BusinessObjects.DTOs.Auth;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName);

public sealed record AuthResponse(
    string Token,
    UserDto User);
