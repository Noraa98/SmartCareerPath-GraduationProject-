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
    public class UserCareerPathProgressConfiguration : IEntityTypeConfiguration<UserCareerPathProgress>
    {
        public void Configure(EntityTypeBuilder<UserCareerPathProgress> builder)
        {
            builder.ToTable("UserCareerPathProgress");

            builder.HasKey(p => p.Id);

            builder.HasIndex(p => new { p.UserCareerPathId, p.TaskId })
                .IsUnique();

            builder.HasOne(p => p.UserCareerPath)
                .WithMany(uc => uc.Progress)
                .HasForeignKey(p => p.UserCareerPathId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Task)
                .WithMany()
                .HasForeignKey(p => p.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
