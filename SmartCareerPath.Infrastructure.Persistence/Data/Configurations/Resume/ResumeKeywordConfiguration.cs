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
    public class ResumeKeywordConfiguration : IEntityTypeConfiguration<ResumeKeyword>
    {
        public void Configure(EntityTypeBuilder<ResumeKeyword> builder)
        {
            builder.ToTable("ResumeKeywords");

            builder.HasKey(k => k.Id);

            builder.Property(k => k.Keyword)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(k => k.Resume)
                .WithMany(r => r.Keywords)
                .HasForeignKey(k => k.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
