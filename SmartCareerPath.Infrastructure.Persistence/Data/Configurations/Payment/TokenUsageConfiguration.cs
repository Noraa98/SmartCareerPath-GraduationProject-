using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Payment
{
    public class TokenUsageConfiguration : IEntityTypeConfiguration<TokenUsage>
    {
        public void Configure(EntityTypeBuilder<TokenUsage> builder)
        {
            builder.ToTable("TokenUsage");

            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Request)
                .WithMany()
                .HasForeignKey(t => t.RequestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

}
