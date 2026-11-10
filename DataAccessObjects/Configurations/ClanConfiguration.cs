using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class ClanConfiguration : IEntityTypeConfiguration<Clan>
{
    public void Configure(EntityTypeBuilder<Clan> builder)
    {
        builder.ToTable("Clans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        // Dữ liệu mẫu để kiểm tra API và frontend.
        // Sẽ được thay bằng seed theo dữ liệu dòng họ thật khi chốt ERD.
        builder.HasData(
            new Clan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Gia phả họ Nguyễn Hữu",
                Description = "Dữ liệu mẫu phục vụ kiểm thử luồng API và giao diện.",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Clan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Gia phả họ Trần",
                Description = "Dữ liệu mẫu phục vụ kiểm thử luồng API và giao diện.",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
