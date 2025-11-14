using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Payment
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(t => t.ProviderReference);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
