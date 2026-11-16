using GiaPha.BusinessObjects.Auditing;
using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.Context;

public sealed class GiaPhaDbContext : DbContext
{
    public GiaPhaDbContext(
        DbContextOptions<GiaPhaDbContext> options,
        IAuditContext? auditContext = null)
        : base(options)
    {
        AuditContext = auditContext;
    }

    /// <summary>
    /// Audit context của request hiện tại. Interceptor đọc từ đây thay vì
    /// giữ một instance scoped trong DbContext options (dễ bị dùng lại giữa request).
    /// </summary>
    public IAuditContext? AuditContext { get; }

    public DbSet<Clan> Clans => Set<Clan>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserClanRole> UserClanRoles => Set<UserClanRole>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<ParentChild> ParentChild => Set<ParentChild>();

    public DbSet<Union> Unions => Set<Union>();

    public DbSet<MagicLinkToken> MagicLinkTokens => Set<MagicLinkToken>();

    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mọi configuration nằm trong DataAccessObjects/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiaPhaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
