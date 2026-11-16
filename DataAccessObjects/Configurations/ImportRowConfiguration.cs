using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class ImportRowConfiguration : IEntityTypeConfiguration<ImportRow>
{
    public void Configure(EntityTypeBuilder<ImportRow> builder)
    {
        builder.ToTable("ImportRows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowIndex)
            .IsRequired();

        builder.Property(x => x.ExtractedJson)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Warnings)
            .HasMaxLength(2000);

        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.MatchedPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BatchId);
        builder.HasIndex(x => new { x.BatchId, x.RowIndex });
        builder.HasIndex(x => x.Status);
    }
}
