using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entities.Payments;
using SmartCareerPath.Domain.Enums;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Payment
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
           
            // Table Configuration

            builder.ToTable("PaymentTransactions");

            builder.HasKey(t => t.Id);


            // Property Configurations
            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.SubscriptionId)
                .IsRequired(false);

            // Enum stored as int
            builder.Property(t => t.Provider)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Payment provider: 1=Stripe, 2=PayPal, 3=Paymob");

            builder.Property(t => t.ProviderReference)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("External transaction ID from payment provider");

            // Money with precision
            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Payment amount in specified currency");

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Currency code: 1=USD, 2=EGP, 3=EUR, etc.");

            builder.Property(t => t.ProductType)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Product type: 1=Interviewer, 2=CV, 3=Bundle, etc.");

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Payment status: 1=Pending, 2=Processing, 3=Completed, etc.");

            builder.Property(t => t.PaymentMethod)
                .IsRequired(false)
                .HasConversion<int>()
                .HasComment("Payment method used by customer");

            builder.Property(t => t.BillingCycle)
                .IsRequired(false)
                .HasConversion<int>();

            builder.Property(t => t.OriginalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.DiscountCode)
                .HasMaxLength(50);

            builder.Property(t => t.TaxAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.CheckoutUrl)
                .HasMaxLength(500);

            builder.Property(t => t.ReceiptUrl)
                .HasMaxLength(500);

            // JSON columns for metadata
            builder.Property(t => t.WebhookPayload)
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.ProviderMetadata)
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.FailureReason)
                .HasMaxLength(500);

            builder.Property(t => t.FailureCode)
                .HasMaxLength(50);

            builder.Property(t => t.RefundReference)
                .HasMaxLength(200);

            builder.Property(t => t.RefundReason)
                .HasMaxLength(500);

            // Timestamps
            builder.Property(t => t.CompletedAt)
                .IsRequired(false);

            builder.Property(t => t.ExpiresAt)
                .IsRequired(false);

            builder.Property(t => t.RefundedAt)
                .IsRequired(false);

            builder.Property(t => t.LastVerifiedAt)
                .IsRequired(false);

           
            // Most common query: find by provider reference
            builder.HasIndex(t => t.ProviderReference)
                .IsUnique()
                .HasDatabaseName("IX_PaymentTransactions_ProviderReference");

            // Query by user
            builder.HasIndex(t => t.UserId)
                .HasDatabaseName("IX_PaymentTransactions_UserId");

            // Query by status (for background jobs)
            builder.HasIndex(t => t.Status)
                .HasDatabaseName("IX_PaymentTransactions_Status");

            // Composite index for user + status queries
            builder.HasIndex(t => new { t.UserId, t.Status })
                .HasDatabaseName("IX_PaymentTransactions_UserId_Status");

            // Query completed payments by date (for reports)
            builder.HasIndex(t => t.CompletedAt)
                .HasDatabaseName("IX_PaymentTransactions_CompletedAt")
                .HasFilter("[CompletedAt] IS NOT NULL");

            // Query expired pending payments (for cleanup jobs)
            builder.HasIndex(t => new { t.Status, t.ExpiresAt })
                .HasDatabaseName("IX_PaymentTransactions_Status_ExpiresAt")
                .HasFilter("[ExpiresAt] IS NOT NULL");

          
            // Relationships
            // Many-to-One: PaymentTransaction -> User
            builder.HasOne(t => t.User)
                .WithMany(u => u.PaymentTransactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict) // Don't cascade delete payments when user is deleted
                .HasConstraintName("FK_PaymentTransactions_Users");

            // Many-to-One: PaymentTransaction -> UserSubscription (nullable)
            builder.HasOne(t => t.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull) // Set to null if subscription is deleted
                .HasConstraintName("FK_PaymentTransactions_UserSubscriptions");

            // ═══════════════════════════════════════════════════════════════
            // Default Values (optional - can be set in entity constructor)
            // ═══════════════════════════════════════════════════════════════

            builder.Property(t => t.Status)
                .HasDefaultValue(PaymentStatus.Pending);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}