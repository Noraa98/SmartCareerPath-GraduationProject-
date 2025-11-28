using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using SmartCareerPath.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Payments
{
    public class PaymentTransaction : BaseEntity
    {
        // Core Payment Information
        [Required]
        public int UserId { get; set; }

      
        public int? SubscriptionId { get; set; }

       
        [Required]
        public PaymentProvider Provider { get; set; }

        [Required]
        [MaxLength(200)]
        public required string ProviderReference { get; set; }

        [Required]
        public decimal Amount { get; set; }

        
        [Required]
        public Currency Currency { get; set; }

        
        [Required]
        public ProductType ProductType { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Payment Details
        public PaymentMethod? PaymentMethod { get; set; }

        
        public BillingCycle? BillingCycle { get; set; }

     
        public decimal? OriginalAmount { get; set; }

        
        [MaxLength(50)]
        public string? DiscountCode { get; set; }

       
        public decimal? TaxAmount { get; set; }

        
        // Provider Integration Details
        [MaxLength(500)]
        public string? CheckoutUrl { get; set; }

     
        [MaxLength(500)]
        public string? ReceiptUrl { get; set; }

      
        public string? WebhookPayload { get; set; }

        
        public string? ProviderMetadata { get; set; }

    
        // Failure & Refund Details
        [MaxLength(500)]
        public string? FailureReason { get; set; }


        [MaxLength(50)]
        public string? FailureCode { get; set; }

        
        [MaxLength(200)]
        public string? RefundReference { get; set; }

        
        [MaxLength(500)]
        public string? RefundReason { get; set; }

       
        public DateTime? RefundedAt { get; set; }

       
        // Timestamps
        
        public DateTime? CompletedAt { get; set; }

       
        public DateTime? ExpiresAt { get; set; }

       
        public DateTime? LastVerifiedAt { get; set; }


        // Navigation Properties
        public virtual User User { get; set; } = null!;

        
        public virtual UserSubscription? Subscription { get; set; }

       
        // Business Logic Methods
        public void MarkAsCompleted(DateTime? completedAt = null)
        {
            Status = PaymentStatus.Completed;
            CompletedAt = completedAt ?? DateTime.UtcNow;
            LastVerifiedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string failureReason, string? failureCode = null)
        {
            Status = PaymentStatus.Failed;
            FailureReason = failureReason;
            FailureCode = failureCode;
            LastVerifiedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded(string refundReference, string? refundReason = null)
        {
            Status = PaymentStatus.Refunded;
            RefundReference = refundReference;
            RefundReason = refundReason;
            RefundedAt = DateTime.UtcNow;
            LastVerifiedAt = DateTime.UtcNow;
        }

       
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        }

      
        public bool CanBeRefunded()
        {
            // Only completed payments can be refunded
            // And not already refunded
            return Status == PaymentStatus.Completed &&
                   string.IsNullOrEmpty(RefundReference);
        }

        
        public string GetDisplayAmount()
        {
            return Currency switch
            {
                Currency.USD => $"${Amount:F2}",
                Currency.EUR => $"€{Amount:F2}",
                Currency.GBP => $"£{Amount:F2}",
                Currency.EGP => $"{Amount:F2} EGP",
                Currency.SAR => $"{Amount:F2} SAR",
                _ => $"{Amount:F2} {Currency}"
            };
        }
    }
}
