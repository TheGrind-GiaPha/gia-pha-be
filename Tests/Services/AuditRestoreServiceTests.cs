using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using Moq;

namespace GiaPha.UnitTests.Services;

public sealed class AuditRestoreServiceTests
{
    [Fact]
    public async Task RestoreAsync_WhenClanNameStillMatches_AppliesOldNameAndSaves()
    {
        var clanId = Guid.NewGuid();
        var clan = new Clan { Id = clanId, Name = "Tên mới", Description = "Mô tả" };
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClanId = clanId,
            EntityType = "Clan",
            EntityId = clanId,
            Action = AuditAction.Update,
            Before = """{"name":"Tên cũ"}""",
            After = """{"name":"Tên mới"}"""
        };

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.GetByIdAsync(log.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(clanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clan);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);
        repositoryManager
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var auditContext = new AuditContext();
        var service = new AuditRestoreService(repositoryManager.Object, auditContext);

        var result = await service.RestoreAsync(clanId, log.Id);

        Assert.Equal("Tên cũ", clan.Name);
        Assert.False(auditContext.IsRestore);
        Assert.Equal(log.Id, result.LogId);
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_WhenCurrentValueDiffersFromAfter_ThrowsConflictException()
    {
        var clanId = Guid.NewGuid();
        var clan = new Clan { Id = clanId, Name = "Đã bị sửa tiếp" };
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClanId = clanId,
            EntityType = "Clan",
            EntityId = clanId,
            Action = AuditAction.Update,
            Before = """{"name":"Tên cũ"}""",
            After = """{"name":"Tên mới"}"""
        };

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.GetByIdAsync(log.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var clanRepository = new Mock<IClanRepository>();
        clanRepository
            .Setup(x => x.GetByIdAsync(clanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clan);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);
        repositoryManager.SetupGet(x => x.Clan).Returns(clanRepository.Object);

        var service = new AuditRestoreService(repositoryManager.Object, new AuditContext());

        await Assert.ThrowsAsync<ConflictException>(() => service.RestoreAsync(clanId, log.Id));
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_WhenLogBelongsToAnotherClan_ThrowsNotFoundException()
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClanId = Guid.NewGuid(),
            EntityType = "Clan",
            Action = AuditAction.Update,
            Before = """{"name":"A"}""",
            After = """{"name":"B"}"""
        };

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.GetByIdAsync(log.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);

        var service = new AuditRestoreService(repositoryManager.Object, new AuditContext());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RestoreAsync(Guid.NewGuid(), log.Id));
    }

    [Fact]
    public async Task RestoreAsync_WhenLogHasStructuralChange_ThrowsConflictException()
    {
        var clanId = Guid.NewGuid();
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClanId = clanId,
            EntityType = "Clan",
            EntityId = clanId,
            Action = AuditAction.Update,
            Before = """{"status":"Draft"}""",
            After = """{"status":"Published"}"""
        };

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.GetByIdAsync(log.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);

        var service = new AuditRestoreService(repositoryManager.Object, new AuditContext());

        await Assert.ThrowsAsync<ConflictException>(() => service.RestoreAsync(clanId, log.Id));
    }

    [Fact]
    public async Task RestoreAsync_WhenPersonNameStillMatches_AppliesOldName()
    {
        var clanId = Guid.NewGuid();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            ClanId = clanId,
            FullName = "Nguyễn Văn B"
        };
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClanId = clanId,
            EntityType = "Person",
            EntityId = person.Id,
            Action = AuditAction.Update,
            Before = """{"fullName":"Nguyễn Văn A"}""",
            After = """{"fullName":"Nguyễn Văn B"}"""
        };

        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(x => x.GetByIdAsync(log.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(person.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.AuditLog).Returns(auditRepository.Object);
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new AuditRestoreService(repositoryManager.Object, new AuditContext());

        await service.RestoreAsync(clanId, log.Id);

        Assert.Equal("Nguyễn Văn A", person.FullName);
    }
}
