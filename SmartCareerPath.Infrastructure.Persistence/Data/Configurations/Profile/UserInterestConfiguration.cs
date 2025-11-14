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
    public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
    {
        public void Configure(EntityTypeBuilder<UserInterest> builder)
        {
            builder.ToTable("UserInterests");

            builder.HasKey(ui => ui.Id);

            builder.HasIndex(ui => new { ui.UserProfileId, ui.InterestId })
                .IsUnique();

            builder.HasOne(ui => ui.UserProfile)
                .WithMany(p => p.UserInterests)
                .HasForeignKey(ui => ui.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ui => ui.Interest)
                .WithMany(i => i.UserInterests)
                .HasForeignKey(ui => ui.InterestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
