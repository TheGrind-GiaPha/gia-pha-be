using System.Text.Json;
using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.Entities;
using GiaPha.DataAccessObjects.Auditing;
using GiaPha.DataAccessObjects.Context;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.UnitTests.Auditing;

public sealed class AuditSaveChangesInterceptorTests
{
    private static GiaPhaDbContext CreateContext(out AuditContext auditContext)
    {
        auditContext = new AuditContext();

        var options = new DbContextOptionsBuilder<GiaPhaDbContext>()
            .UseInMemoryDatabase($"audit-{Guid.NewGuid()}")
            .AddInterceptors(new AuditSaveChangesInterceptor(auditContext))
            .Options;

        return new GiaPhaDbContext(options);
    }

    [Fact]
    public async Task SavingAddedPerson_WritesOneAuditLog()
    {
        await using var db = CreateContext(out _);

        var clanId = Guid.NewGuid();
        var person = new Person
        {
            ClanId = clanId,
            FullName = "Nguyễn Văn A",
            Generation = 1,
            Path = "/"
        };

        db.Persons.Add(person);
        await db.SaveChangesAsync();

        var log = db.AuditLogs.Single();

        Assert.Equal(clanId, log.ClanId);
        Assert.Equal("Person", log.EntityType);
        Assert.Equal(person.Id, log.EntityId);
        Assert.Equal(AuditAction.Create, log.Action);
        Assert.Null(log.Before);

        using var after = JsonDocument.Parse(log.After!);
        Assert.Equal("Nguyễn Văn A", after.RootElement.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task SavingModifiedClan_LogsOnlyChangedFields()
    {
        await using var db = CreateContext(out _);

        var clan = new Clan
        {
            Name = "Gia phả họ Nguyễn Hữu",
            Description = "Mô tả cũ"
        };

        db.Clans.Add(clan);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var tracked = await db.Clans.SingleAsync(x => x.Id == clan.Id);
        tracked.Name = "Gia phả họ Nguyễn Hữu 1";
        tracked.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var log = db.AuditLogs.Single(x => x.Action == AuditAction.Update);

        Assert.Equal("Clan", log.EntityType);
        Assert.Equal(clan.Id, log.EntityId);

        using var before = JsonDocument.Parse(log.Before!);
        using var after = JsonDocument.Parse(log.After!);

        Assert.Equal(
            new[] { "name" },
            before.RootElement.EnumerateObject().Select(x => x.Name));

        Assert.Equal("Gia phả họ Nguyễn Hữu", before.RootElement.GetProperty("name").GetString());
        Assert.Equal("Gia phả họ Nguyễn Hữu 1", after.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task SavingClanMarkedModifiedByRepository_LogsOnlyActuallyChangedFields()
    {
        await using var db = CreateContext(out _);

        var clan = new Clan { Name = "A", Description = "Mô tả" };
        db.Clans.Add(clan);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var tracked = await db.Clans.SingleAsync(x => x.Id == clan.Id);
        tracked.Name = "B";
        db.Clans.Update(tracked);
        await db.SaveChangesAsync();

        var log = db.AuditLogs.Single(x => x.Action == AuditAction.Update);

        using var before = JsonDocument.Parse(log.Before!);
        using var after = JsonDocument.Parse(log.After!);

        Assert.Equal(
            new[] { "name" },
            before.RootElement.EnumerateObject().Select(x => x.Name));
        Assert.Equal(
            new[] { "name" },
            after.RootElement.EnumerateObject().Select(x => x.Name));
    }

    [Fact]
    public async Task SavingModifiedClan_WhenRestoreFlagSet_LogsRestoreAction()
    {
        await using var db = CreateContext(out var auditContext);

        var clan = new Clan { Name = "A" };
        db.Clans.Add(clan);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var tracked = await db.Clans.SingleAsync(x => x.Id == clan.Id);
        tracked.Name = "B";
        auditContext.IsRestore = true;
        await db.SaveChangesAsync();

        var log = await db.AuditLogs.SingleAsync(x => x.Action == AuditAction.Restore);

        Assert.Equal("Clan", log.EntityType);
        using var before = JsonDocument.Parse(log.Before!);
        Assert.Equal("A", before.RootElement.GetProperty("name").GetString());
    }
}
