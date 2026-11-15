using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class ParentChildConfiguration : IEntityTypeConfiguration<ParentChild>
{
    public void Configure(EntityTypeBuilder<ParentChild> builder)
    {
        builder.ToTable("ParentChild");

        builder.HasKey(x => x.Id);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ParentId, x.ChildId })
            .IsUnique();
    }
}
