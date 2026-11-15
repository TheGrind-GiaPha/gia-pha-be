using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiaPha.DataAccessObjects.Configurations;

public sealed class UserClanRoleConfiguration : IEntityTypeConfiguration<UserClanRole>
{
    public void Configure(EntityTypeBuilder<UserClanRole> builder)
    {
        builder.ToTable("UserClanRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Clan>()
            .WithMany()
            .HasForeignKey(x => x.ClanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mỗi người chỉ có 1 bản ghi cho mỗi vai trò trong 1 dòng họ.
        builder.HasIndex(x => new { x.UserId, x.ClanId, x.Role })
            .IsUnique();
    }
}
