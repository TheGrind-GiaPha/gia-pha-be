using GiaPha.BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace GiaPha.DataAccessObjects.Configurations;
public sealed class PersonProfileConfiguration : IEntityTypeConfiguration<PersonProfileExtension>
{
 public void Configure(EntityTypeBuilder<PersonProfileExtension> b)
 {
 b.ToTable("PersonProfileExtensions"); b.HasKey(x=>x.PersonId); b.HasIndex(x=>x.ClanId);
 b.HasOne<Person>().WithOne().HasForeignKey<PersonProfileExtension>(x=>x.PersonId).OnDelete(DeleteBehavior.Cascade);
 b.Property(x=>x.Biography).HasMaxLength(4000); b.Property(x=>x.BurialPlace).HasMaxLength(500);
 b.Property(x=>x.BurialMapUrl).HasMaxLength(1000); b.Property(x=>x.PhoneNumber).HasMaxLength(30);
 }
}
public sealed class PersonPhotoConfiguration : IEntityTypeConfiguration<PersonPhoto>
{
 public void Configure(EntityTypeBuilder<PersonPhoto> b)
 {
 b.ToTable("PersonPhotos"); b.HasKey(x=>x.Id); b.HasIndex(x=>x.ClanId); b.HasIndex(x=>x.PersonId);
 b.HasOne<Person>().WithMany().HasForeignKey(x=>x.PersonId).OnDelete(DeleteBehavior.Cascade);
 b.Property(x=>x.ObjectKey).HasMaxLength(500); b.Property(x=>x.Url).HasMaxLength(1000); b.Property(x=>x.Caption).HasMaxLength(300);
 b.Property(x=>x.ContentType).HasMaxLength(100); b.Property(x=>x.IsOrphan).HasDefaultValue(true);
 b.Property(x=>x.IsVerified).HasDefaultValue(false); b.Property(x=>x.SetAsPortrait).HasDefaultValue(false);
 }
}
