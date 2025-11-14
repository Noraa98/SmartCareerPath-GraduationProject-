using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Subscription
{
    public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.ToTable("UserSubscriptions");

            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Plan)
                .WithMany(p => p.UserSubscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
