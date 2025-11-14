using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.CVTemplatesAndExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.AIAndTemplates
{
    public class CVExportConfiguration : IEntityTypeConfiguration<CVExport>
    {
        public void Configure(EntityTypeBuilder<CVExport> builder)
        {
            builder.ToTable("CVExports");

            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Resume)
                .WithMany()
                .HasForeignKey(e => e.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
