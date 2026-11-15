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

    public DbSet<User> Users => Set<User>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<ParentChild> ParentChild => Set<ParentChild>();

    public DbSet<Union> Unions => Set<Union>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mọi configuration nằm trong DataAccessObjects/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiaPhaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
