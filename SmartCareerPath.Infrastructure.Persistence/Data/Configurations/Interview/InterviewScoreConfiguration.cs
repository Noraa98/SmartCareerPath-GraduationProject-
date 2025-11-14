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
    public class InterviewScoreConfiguration : IEntityTypeConfiguration<InterviewScore>
    {
        public void Configure(EntityTypeBuilder<InterviewScore> builder)
        {
            builder.ToTable("InterviewScores");

            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.InterviewSessionId)
                .IsUnique();
        }
    }
}
