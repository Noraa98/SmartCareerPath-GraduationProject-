using SmartCareerPath.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Payment
{
    public interface IPaymentStrategy
    {
        /// <summary>
        /// Provider type this strategy handles.
        /// </summary>
        PaymentProvider Provider { get; }

        /// <summary>
        /// Create payment session/checkout URL.
        /// </summary>
        Task<PaymentSessionResult> CreatePaymentSessionAsync(
            CreatePaymentSessionParams sessionParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify webhook signature from provider.
        /// </summary>
        bool VerifyWebhookSignature(string payload, string signature, string secret);

        /// <summary>
        /// Parse webhook payload to extract payment status.
        /// </summary>
        WebhookPaymentInfo ParseWebhookPayload(string payload);

        /// <summary>
        /// Get payment status from provider API (for reconciliation).
        /// </summary>
        Task<ProviderPaymentStatus> GetPaymentStatusAsync(
            string providerReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process refund through provider API.
        /// </summary>
        Task<RefundResult> ProcessRefundAsync(
            string providerReference,
            decimal amount,
            Currency currency,
            string reason,
            CancellationToken cancellationToken = default);
    }

    // ═══════════════════════════════════════════════════════════════
    // Supporting DTOs for Strategy Pattern
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Parameters for creating payment session.
    /// </summary>
    public class CreatePaymentSessionParams
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public ProductType ProductType { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? SuccessUrl { get; set; }
        public string? CancelUrl { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Result from creating payment session.
    /// </summary>
    public class PaymentSessionResult
    {
        public bool Success { get; set; }
        public string? ProviderReference { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> ProviderMetadata { get; set; } = new();
    }

    /// <summary>
    /// Parsed webhook payment information.
    /// </summary>
    public class WebhookPaymentInfo
    {
        public string ProviderReference { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Provider payment status response.
    /// </summary>
    public class ProviderPaymentStatus
    {
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Refund operation result.
    /// </summary>
    public class RefundResult
    {
        public bool Success { get; set; }
        public string? RefundReference { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
