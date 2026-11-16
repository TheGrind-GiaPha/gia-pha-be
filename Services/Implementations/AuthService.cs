using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GiaPha.BusinessObjects.DTOs.Auth;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;
using GiaPha.Services.Options;
using GiaPha.Services.Security;
using Microsoft.IdentityModel.Tokens;

namespace GiaPha.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly JwtSettings _jwt;

    public AuthService(IRepositoryManager repository, ILoggerManager logger, JwtSettings jwt)
    {
        _repository = repository;
        _logger = logger;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (await _repository.User.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email này đã được đăng ký.");
        }

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        _repository.User.Create(user);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã đăng ký tài khoản {email}.");
        return new AuthResponse(GenerateToken(user), Map(user));
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _repository.User.GetByEmailAsync(email, cancellationToken);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");
        }

        _logger.LogInfo($"Đăng nhập thành công: {email}.");
        return new AuthResponse(GenerateToken(user), Map(user));
    }

    public async Task<MagicLinkResponse> RequestMagicLinkAsync(
        MagicLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _repository.User.GetByEmailAsync(email, cancellationToken);

        // Không tiết lộ email có tồn tại hay không.
        if (user is null)
        {
            return new MagicLinkResponse(null, "Nếu email tồn tại, bạn sẽ nhận được liên kết đăng nhập.");
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var tokenHash = HashToken(token);

        _repository.MagicLinkToken.Create(new MagicLinkToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(48)
        });
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã tạo magic link cho {email}.");

        // Dev trả link để demo; production gửi qua email/Zalo, không trả link.
        return new MagicLinkResponse($"/m/{token}", "Đã tạo liên kết đăng nhập (hết hạn 48 giờ).");
    }

    public async Task<AuthResponse?> VerifyMagicLinkAsync(
        MagicLinkVerifyRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.Token);
        var link = await _repository.MagicLinkToken.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (link is null || link.UsedAt is not null || link.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        var user = await _repository.User.GetByIdAsync(link.UserId, trackChanges: false, cancellationToken);
        if (user is null)
        {
            return null;
        }

        link.UsedAt = DateTime.UtcNow;
        _repository.MagicLinkToken.Update(link);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Magic link đã được dùng cho {user.Email}.");
        return new AuthResponse(GenerateToken(user), Map(user));
    }

    public async Task<UserDto?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _repository.User.GetByIdAsync(userId, trackChanges: false, cancellationToken);
        return user is null ? null : Map(user);
    }

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static UserDto Map(User user) => new(user.Id, user.Email, user.DisplayName);
}
