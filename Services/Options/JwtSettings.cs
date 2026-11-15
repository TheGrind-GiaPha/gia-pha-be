namespace GiaPha.Services.Options;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 480;
}
