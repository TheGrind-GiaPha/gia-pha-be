using GiaPha.BusinessObjects.DTOs.Audits;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.BusinessObjects.Queries;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using Moq;

namespace GiaPha.UnitTests.Services;

public sealed class AuditLogServiceTests
{
    [Fact]
    public async Task GetByClanAsync_WhenClanMissing_ThrowsNotFoundException()
    {
        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Clan?)null);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);

        var service = new AuditLogService(repositoryManager.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByClanAsync(Guid.NewGuid(), new AuditLogQuery()));
    }

    [Fact]
    public async Task GetByClanAsync_ClampsPageSizeAndMapsLogs()
    {
        var clanId = Guid.NewGuid();
        var log = new AuditLog
        {
            ClanId = clanId,
            EntityType = "Clan",
            Action = AuditAction.Update,
            Before = """{"name":"A"}""",
            After = """{"name":"B"}"""
        };

        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(clanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Clan { Id = clanId, Name = "A" });

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.CountByClanAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        auditRepository
            .Setup(x => x.GetByClanAsync(It.IsAny<AuditLogFilter>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog> { log });

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);

        var service = new AuditLogService(repositoryManager.Object);

        var result = await service.GetByClanAsync(clanId, new AuditLogQuery
        {
            Page = 0,
            PageSize = 500
        });

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(1, result.Total);
        var item = Assert.Single(result.Items);
        Assert.Equal("Cập nhật dòng họ B", item.Summary);
    }

    [Fact]
    public async Task GetByClanAsync_PassesNormalizedFiltersToRepository()
    {
        var clanId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(clanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Clan { Id = clanId, Name = "A" });

        AuditLogFilter? captured = null;
        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.CountByClanAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogFilter, CancellationToken>((filter, _) => captured = filter)
            .ReturnsAsync(0);
        auditRepository
            .Setup(x => x.GetByClanAsync(It.IsAny<AuditLogFilter>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>());

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);

        var service = new AuditLogService(repositoryManager.Object);

        await service.GetByClanAsync(clanId, new AuditLogQuery
        {
            EntityType = "person",
            EntityId = personId,
            ActorId = actorId,
            From = from,
            To = to,
            Page = 2,
            PageSize = 50
        });

        Assert.NotNull(captured);
        Assert.Equal(clanId, captured!.ClanId);
        Assert.Equal("Person", captured.EntityType);
        Assert.Equal(personId, captured.EntityId);
        Assert.Equal(actorId, captured.ActorId);
        Assert.Equal(from, captured.From);
        Assert.Equal(to, captured.To);
        Assert.Equal(2, captured.Page);
        Assert.Equal(50, captured.PageSize);
    }

    [Fact]
    public async Task GetPersonHistoryAsync_WhenPersonBelongsToAnotherClan_ThrowsNotFoundException()
    {
        var clanId = Guid.NewGuid();
        var person = new Person { Id = Guid.NewGuid(), ClanId = Guid.NewGuid() };

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(person.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);

        var service = new AuditLogService(repositoryManager.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPersonHistoryAsync(clanId, person.Id, new AuditLogQuery()));
    }
}
