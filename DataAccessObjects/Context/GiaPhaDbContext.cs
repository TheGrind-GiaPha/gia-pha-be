using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.DataAccessObjects.Context;

public sealed class GiaPhaDbContext : DbContext
{
    public GiaPhaDbContext(DbContextOptions<GiaPhaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Clan> Clans => Set<Clan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mọi configuration nằm trong DataAccessObjects/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiaPhaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
