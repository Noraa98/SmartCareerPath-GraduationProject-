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
    public class AIRequestConfiguration : IEntityTypeConfiguration<AIRequest>
    {
        public void Configure(EntityTypeBuilder<AIRequest> builder)
        {
            builder.ToTable("AIRequests");

            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.User)
                .WithMany(u => u.AIRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(r => r.CreatedAt);
        }
    }
}
