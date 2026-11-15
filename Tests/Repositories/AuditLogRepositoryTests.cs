using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Queries;
using GiaPha.DataAccessObjects.Context;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.UnitTests.Repositories;

public sealed class AuditLogRepositoryTests
{
    [Fact]
    public async Task GetByClanAsync_ReturnsOnlyLogsOfRequestedClan()
    {
        await using var db = CreateContext();
        var clanA = Guid.NewGuid();
        var clanB = Guid.NewGuid();
        var personId = Guid.NewGuid();

        db.AuditLogs.AddRange(
            Log(clanA, "Person", personId, DateTime.UtcNow),
            Log(clanA, "Person", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5)),
            Log(clanA, "Clan", clanA, DateTime.UtcNow.AddMinutes(-10)),
            Log(clanB, "Person", personId, DateTime.UtcNow));
        await db.SaveChangesAsync();

        var repository = new AuditLogRepository(new AuditLogDao(db));

        var logs = await repository.GetByClanAsync(
            new AuditLogFilter { ClanId = clanA, Page = 1, PageSize = 10 },
            trackChanges: false);
        var total = await repository.CountByClanAsync(new AuditLogFilter { ClanId = clanA });

        Assert.Equal(3, logs.Count);
        Assert.All(logs, log => Assert.Equal(clanA, log.ClanId));
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task GetByClanAsync_FiltersEntityAndPaginatesNewestFirst()
    {
        await using var db = CreateContext();
        var clanId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var older = Log(clanId, "Person", personId, DateTime.UtcNow.AddMinutes(-10));
        var newer = Log(clanId, "Person", personId, DateTime.UtcNow);

        db.AuditLogs.AddRange(
            older,
            newer,
            Log(clanId, "Clan", clanId, DateTime.UtcNow.AddMinutes(-20)));
        await db.SaveChangesAsync();

        var repository = new AuditLogRepository(new AuditLogDao(db));

        var firstPage = await repository.GetByClanAsync(
            new AuditLogFilter
            {
                ClanId = clanId,
                EntityType = "Person",
                EntityId = personId,
                Page = 1,
                PageSize = 1
            },
            trackChanges: false);

        var secondPage = await repository.GetByClanAsync(
            new AuditLogFilter
            {
                ClanId = clanId,
                EntityType = "Person",
                EntityId = personId,
                Page = 2,
                PageSize = 1
            },
            trackChanges: false);

        Assert.Equal(newer.Id, Assert.Single(firstPage).Id);
        Assert.Equal(older.Id, Assert.Single(secondPage).Id);

        var total = await repository.CountByClanAsync(new AuditLogFilter
        {
            ClanId = clanId,
            EntityType = "Person",
            EntityId = personId
        });

        Assert.Equal(2, total);
    }

    private static GiaPhaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GiaPhaDbContext>()
            .UseInMemoryDatabase($"audit-repository-{Guid.NewGuid()}")
            .Options;

        return new GiaPhaDbContext(options);
    }

    private static AuditLog Log(Guid clanId, string entityType, Guid entityId, DateTime occurredAt)
        => new()
        {
            ClanId = clanId,
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Update,
            BatchId = Guid.NewGuid(),
            OccurredAt = occurredAt
        };
}
