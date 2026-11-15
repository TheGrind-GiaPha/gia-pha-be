using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BeforeJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AfterJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.HasIndex(x => x.ClanId);
        builder.HasIndex(x => new { x.ClanId, x.EntityType, x.EntityId });
    }
}
