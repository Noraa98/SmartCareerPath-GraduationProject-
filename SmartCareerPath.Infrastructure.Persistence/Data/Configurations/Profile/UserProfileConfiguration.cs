using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Profile
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.UserId)
                .IsUnique();

            builder.Property(p => p.CurrentRole)
                .HasMaxLength(200);

            builder.Property(p => p.City)
                .HasMaxLength(200);

            builder.Property(p => p.Country)
                .HasMaxLength(100);
        }
    }
}
