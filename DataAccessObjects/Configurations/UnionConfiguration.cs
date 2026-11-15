using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class UnionConfiguration : IEntityTypeConfiguration<Union>
{
    public void Configure(EntityTypeBuilder<Union> builder)
    {
        builder.ToTable("Unions");

        builder.HasKey(x => x.Id);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.HusbandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.WifeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.HusbandId, x.WifeId })
            .IsUnique();
    }
}
