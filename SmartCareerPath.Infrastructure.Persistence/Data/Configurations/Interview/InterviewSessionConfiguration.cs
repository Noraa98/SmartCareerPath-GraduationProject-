using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.InterviewSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Interview
{
    public class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
    {
        public void Configure(EntityTypeBuilder<InterviewSession> builder)
        {
            builder.ToTable("InterviewSessions");

            builder.HasKey(i => i.Id);

            builder.HasIndex(i => i.SessionId)
                .IsUnique();

            builder.HasOne(i => i.User)
                .WithMany(u => u.InterviewSessions)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.JobApplication)
                .WithMany()
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(i => i.Score)
                .WithOne(s => s.InterviewSession)
                .HasForeignKey<InterviewScore>(s => s.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
