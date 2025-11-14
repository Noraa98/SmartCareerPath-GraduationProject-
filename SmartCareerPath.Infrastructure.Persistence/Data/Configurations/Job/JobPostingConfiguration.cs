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
    public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
    {
        public void Configure(EntityTypeBuilder<JobPosting> builder)
        {
            builder.ToTable("JobPostings");

            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(j => j.Company)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(j => j.Employer)
                .WithMany(u => u.JobPostings)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(j => j.PostedDate);
            builder.HasIndex(j => j.IsActive);
        }
    }

}
