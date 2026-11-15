using GiaPha.BusinessObjects.DTOs.Auth;

namespace GiaPha.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
