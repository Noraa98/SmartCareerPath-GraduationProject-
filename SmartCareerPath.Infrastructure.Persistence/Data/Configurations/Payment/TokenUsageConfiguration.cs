using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;

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
