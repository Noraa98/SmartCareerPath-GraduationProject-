using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Resume
{
    public class ResumeSuggestionConfiguration : IEntityTypeConfiguration<ResumeSuggestion>
    {
        public void Configure(EntityTypeBuilder<ResumeSuggestion> builder)
        {
            builder.ToTable("ResumeSuggestions");

            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Resume)
                .WithMany(r => r.Suggestions)
                .HasForeignKey(s => s.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
