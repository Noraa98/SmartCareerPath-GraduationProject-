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
    public class ResumeScoreConfiguration : IEntityTypeConfiguration<ResumeScore>
    {
        public void Configure(EntityTypeBuilder<ResumeScore> builder)
        {
            builder.ToTable("ResumeScores");

            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Resume)
                .WithMany(r => r.Scores)
                .HasForeignKey(s => s.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
