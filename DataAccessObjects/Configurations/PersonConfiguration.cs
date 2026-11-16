using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.HuyName)
            .HasMaxLength(200);

        builder.Property(x => x.TuName)
            .HasMaxLength(200);

        builder.Property(x => x.ThuyName)
            .HasMaxLength(200);

        builder.Property(x => x.OtherNames)
            .HasMaxLength(500);

        builder.Property(x => x.Biography)
            .HasMaxLength(4000);

        builder.Property(x => x.BurialPlace)
            .HasMaxLength(500);

        builder.Property(x => x.BurialMapUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Contact)
            .HasMaxLength(500);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(1000);

        // Ngày sinh / ngày mất là owned type: không phải bảng riêng,
        // cho phép thiếu ngày hoặc thiếu tháng.
        builder.OwnsOne(x => x.BirthDate);
        builder.OwnsOne(x => x.DeathDate);

        builder.HasOne<Clan>()
            .WithMany()
            .HasForeignKey(x => x.ClanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClanId);
        builder.HasIndex(x => x.Path);
    }
}
