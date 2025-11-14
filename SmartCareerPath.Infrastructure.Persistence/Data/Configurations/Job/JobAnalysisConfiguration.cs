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
    public class JobAnalysisConfiguration : IEntityTypeConfiguration<JobAnalysis>
    {
        public void Configure(EntityTypeBuilder<JobAnalysis> builder)
        {
            builder.ToTable("JobAnalyses");

            builder.HasKey(ja => ja.Id);

            builder.HasOne(ja => ja.User)
                .WithMany()
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(ja => ja.JobPosting)
                .WithMany(j => j.Analyses)
                .HasForeignKey(ja => ja.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ja => ja.Resume)
                .WithMany()
                .HasForeignKey(ja => ja.ResumeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
