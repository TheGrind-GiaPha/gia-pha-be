using GiaPha.BusinessObjects.DTOs.Auth;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using GiaPha.Services.Logging;
using GiaPha.Services.Options;
using Moq;

namespace GiaPha.UnitTests.Services;

public sealed class AuthServiceTests
{
    private readonly JwtSettings _jwt = new()
    {
        Issuer = "VGMS",
        Audience = "VGMS",
        Secret = "DEV_ONLY_change_me_0123456789abcdef0123456789abcdef",
        ExpiresMinutes = 60
    };

    [Fact]
    public async Task LoginAsync_WithValidPassword_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@giapha.vn",
            DisplayName = "Quản trị viên Hệ thống",
            PasswordHash = "XFTgKkrm2dRo2AAiFTtFyg==.E62KCjyMOwnIxlle8hKgHzjGs6rIbGnOQec9UE/7pFs="
        };

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(x => x.GetByEmailAsync("admin@giapha.vn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var repoMgr = new Mock<IRepositoryManager>();
        repoMgr.SetupGet(x => x.User).Returns(userRepo.Object);

        var service = new AuthService(repoMgr.Object, Mock.Of<ILoggerManager>(), _jwt);

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@giapha.vn",
            Password = "Admin@123"
        });

        Assert.NotNull(response.Token);
        Assert.Equal("admin@giapha.vn", response.User.Email);
        Assert.Equal("Quản trị viên Hệ thống", response.User.DisplayName);
    }
}
