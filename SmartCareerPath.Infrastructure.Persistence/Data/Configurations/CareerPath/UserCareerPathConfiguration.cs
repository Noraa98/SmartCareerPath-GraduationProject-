using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.CareerPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.CareerPath
{
    public class UserCareerPathConfiguration : IEntityTypeConfiguration<UserCareerPath>
    {
        public void Configure(EntityTypeBuilder<UserCareerPath> builder)
        {
            builder.ToTable("UserCareerPaths");

            builder.HasKey(uc => uc.Id);

            builder.HasOne(uc => uc.User)
                .WithMany(u => u.CareerPaths)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uc => uc.CareerPath)
                .WithMany(c => c.UserCareerPaths)
                .HasForeignKey(uc => uc.CareerPathId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
