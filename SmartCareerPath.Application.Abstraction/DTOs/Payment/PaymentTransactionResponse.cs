using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PaymentTransactionResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? SubscriptionId { get; set; }
        public required string Provider { get; set; }
        public required string ProviderReference { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string ProductType { get; set; }
        public required string Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BillingCycle { get; set; }
        public decimal? OriginalAmount { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? FailureReason { get; set; }
        public string? RefundReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
