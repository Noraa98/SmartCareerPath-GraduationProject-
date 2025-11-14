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
    public class InterviewQuestionConfiguration : IEntityTypeConfiguration<InterviewQuestion>
    {
        public void Configure(EntityTypeBuilder<InterviewQuestion> builder)
        {
            builder.ToTable("InterviewQuestions");

            builder.HasKey(q => q.Id);

            builder.HasOne(q => q.InterviewSession)
                .WithMany(i => i.Questions)
                .HasForeignKey(q => q.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
