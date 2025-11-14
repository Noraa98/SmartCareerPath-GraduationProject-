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
    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.ToTable("JobApplications");

            builder.HasKey(ja => ja.Id);

            builder.Property(ja => ja.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(ja => ja.User)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(ja => ja.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ja => ja.JobPosting)
                .WithMany(j => j.Applications)
                .HasForeignKey(ja => ja.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ja => ja.Resume)
                .WithMany(r => r.JobApplications)
                .HasForeignKey(ja => ja.ResumeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(ja => new { ja.UserId, ja.JobPostingId })
                .IsUnique();
        }
    }

}
