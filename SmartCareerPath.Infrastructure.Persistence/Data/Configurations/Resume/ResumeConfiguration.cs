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
    public class ResumeConfiguration : IEntityTypeConfiguration<Domain.Entities.ResumeAndParsing.Resume>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.ResumeAndParsing.Resume> builder)
        {
            builder.ToTable("Resumes");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .HasMaxLength(200);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.ParsingResult)
                .WithOne(p => p.Resume)
                .HasForeignKey<ResumeParsingResult>(p => p.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
