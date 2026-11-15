using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne<Clan>()
            .WithMany()
            .HasForeignKey(x => x.ClanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.FounderPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClanId);
    }
}
