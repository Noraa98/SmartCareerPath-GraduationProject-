using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Auth
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.FullName)
                .HasMaxLength(150);

            builder.Property(u => u.Phone)
                .HasMaxLength(20);

            // Relationships
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query Filter for Soft Delete
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
