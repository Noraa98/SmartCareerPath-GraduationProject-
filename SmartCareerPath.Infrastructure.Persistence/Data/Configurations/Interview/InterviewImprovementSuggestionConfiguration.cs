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
    public class InterviewImprovementSuggestionConfiguration : IEntityTypeConfiguration<InterviewImprovementSuggestion>
    {
        public void Configure(EntityTypeBuilder<InterviewImprovementSuggestion> builder)
        {
            builder.ToTable("InterviewImprovementSuggestions");

            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.InterviewSession)
                .WithMany(i => i.Improvements)
                .HasForeignKey(s => s.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
