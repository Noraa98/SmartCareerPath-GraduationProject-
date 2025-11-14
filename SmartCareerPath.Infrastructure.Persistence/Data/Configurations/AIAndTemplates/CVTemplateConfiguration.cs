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
    public class CVTemplateConfiguration : IEntityTypeConfiguration<CVTemplate>
    {
        public void Configure(EntityTypeBuilder<CVTemplate> builder)
        {
            builder.ToTable("CVTemplates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
