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
    public class InterviewKeywordConfiguration : IEntityTypeConfiguration<InterviewKeyword>
    {
        public void Configure(EntityTypeBuilder<InterviewKeyword> builder)
        {
            builder.ToTable("InterviewKeywords");

            builder.HasKey(k => k.Id);

            builder.HasOne(k => k.InterviewSession)
                .WithMany(i => i.Keywords)
                .HasForeignKey(k => k.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
