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
    public class ResumeParsingResultConfiguration : IEntityTypeConfiguration<ResumeParsingResult>
    {
        public void Configure(EntityTypeBuilder<ResumeParsingResult> builder)
        {
            builder.ToTable("ResumeParsingResults");

            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.ResumeId)
                .IsUnique();
        }
    }
}
