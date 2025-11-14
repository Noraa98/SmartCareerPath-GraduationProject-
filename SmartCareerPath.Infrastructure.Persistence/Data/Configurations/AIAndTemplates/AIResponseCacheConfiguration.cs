using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.AIEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.AIAndTemplates
{
    public class AIResponseCacheConfiguration : IEntityTypeConfiguration<AIResponseCache>
    {
        public void Configure(EntityTypeBuilder<AIResponseCache> builder)
        {
            builder.ToTable("AIResponseCache");

            builder.HasKey(c => c.Id);

            builder.HasIndex(c => c.HashKey)
                .IsUnique();

            builder.Property(c => c.HashKey)
                .IsRequired()
                .HasMaxLength(64);
        }
    }
}
