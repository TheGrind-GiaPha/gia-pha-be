using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Source)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne<Clan>()
            .WithMany()
            .HasForeignKey(x => x.ClanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Rows)
            .WithOne(x => x.Batch)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ClanId);
        builder.HasIndex(x => x.Status);
    }
}
