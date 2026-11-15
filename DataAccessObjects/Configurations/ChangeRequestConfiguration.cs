using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class ChangeRequestConfiguration : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(EntityTypeBuilder<ChangeRequest> builder)
    {
        builder.ToTable("ChangeRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.ReviewNote)
            .HasMaxLength(1000);

        builder.HasOne<Clan>()
            .WithMany()
            .HasForeignKey(x => x.ClanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClanId);
        builder.HasIndex(x => x.Status);
    }
}
