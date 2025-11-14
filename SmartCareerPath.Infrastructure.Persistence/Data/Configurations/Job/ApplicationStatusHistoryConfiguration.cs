using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Job
{
    public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
        {
            builder.ToTable("ApplicationStatusHistory");

            builder.HasKey(h => h.Id);

            builder.HasOne(h => h.JobApplication)
                .WithMany(ja => ja.StatusHistory)
                .HasForeignKey(h => h.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
