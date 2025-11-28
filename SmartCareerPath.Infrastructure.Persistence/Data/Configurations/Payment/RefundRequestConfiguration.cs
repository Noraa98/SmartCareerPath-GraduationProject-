using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCareerPath.Domain.Entities.Payments;
using SmartCareerPath.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Payment
{
    public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
    {
        public void Configure(EntityTypeBuilder<RefundRequest> builder)
        {
            builder.ToTable("RefundRequests");

            builder.HasKey(r => r.Id);

            
            // Properties
            builder.Property(r => r.RefundAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Currency)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(r => r.Reason)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(RefundStatus.Requested);

            builder.Property(r => r.AdminNotes)
                .HasMaxLength(1000);

            builder.Property(r => r.ProviderRefundReference)
                .HasMaxLength(200);

            builder.Property(r => r.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(r => r.RequestedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

         
            // Indexes
            builder.HasIndex(r => r.PaymentTransactionId)
                .HasDatabaseName("IX_RefundRequests_PaymentTransactionId");

            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_RefundRequests_UserId");

            builder.HasIndex(r => r.Status)
                .HasDatabaseName("IX_RefundRequests_Status");

            builder.HasIndex(r => new { r.Status, r.RequestedAt })
                .HasDatabaseName("IX_RefundRequests_Status_RequestedAt");

            // Relationships
            builder.HasOne(r => r.PaymentTransaction)
                .WithMany() // No navigation property on PaymentTransaction
                .HasForeignKey(r => r.PaymentTransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RefundRequests_PaymentTransactions");

            builder.HasOne(r => r.User)
                .WithMany() // No navigation property on User
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RefundRequests_Users");

            builder.HasOne(r => r.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_RefundRequests_Admins");
        }
    }
}
