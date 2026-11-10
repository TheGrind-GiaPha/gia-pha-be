using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using GiaPha.Services.Logging;
using Moq;

namespace GiaPha.UnitTests.Services;

public sealed class ClanServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsConflictException()
    {
        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.ExistsByNameAsync("Họ Nguyễn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);

        var service = new ClanService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateClanRequest { Name = "  Họ Nguyễn  " }));

        clanRepository.Verify(x => x.Create(It.IsAny<Clan>()), Times.Never);
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsNew_TrimsNameAndSaves()
    {
        Clan? createdClan = null;

        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.ExistsByNameAsync("Họ Nguyễn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        clanRepository
            .Setup(x => x.Create(It.IsAny<Clan>()))
            .Callback<Clan>(clan => createdClan = clan);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);
        repositoryManager
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ClanService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var result = await service.CreateAsync(new CreateClanRequest
        {
            Name = "  Họ Nguyễn  ",
            Description = "  Dòng họ mẫu  "
        });

        Assert.Equal("Họ Nguyễn", result.Name);
        Assert.Equal("Dòng họ mẫu", result.Description);
        Assert.NotNull(createdClan);
        Assert.Equal("Họ Nguyễn", createdClan!.Name);
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenClanDoesNotExist_ReturnsFalse()
    {
        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Clan?)null);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);

        var service = new ClanService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
