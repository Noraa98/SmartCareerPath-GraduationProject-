using SmartCareerPath.Application.Abstraction.DTOs.Payment;
using SmartCareerPath.Domain.Common.ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Payment
{
    public interface IPaymentService
    {
        Task<Result<PaymentSessionResponse>> CreatePaymentSessionAsync(CreatePaymentSessionRequest request, CancellationToken cancellationToken = default);

        Task<Result<PaymentVerificationResponse>> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken cancellationToken = default);

        Task<Result<PaymentTransactionResponse>> GetPaymentByIdAsync(int paymentId, CancellationToken cancellationToken = default);

        Task<Result<PaginatedResponse<PaymentHistoryItemResponse>>> GetUserPaymentHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<Result<RefundRequestResponse>> CreateRefundRequestAsync(CreateRefundRequest request, CancellationToken cancellationToken = default);

        Task<Result<List<ProductPricingResponse>>> GetProductPricingAsync(int currency, CancellationToken cancellationToken = default);

        Task<Result> HandleWebhookEventAsync(int provider, string payload, string signature, CancellationToken cancellationToken = default);

        // Development/testing helper: force-activate a payment by id.
        // This will set the payment back to Pending and attempt verification so
        // ActivateSubscriptionAsync runs (use only in dev environments).
        Task<Result> ForceActivatePaymentAsync(int paymentId, CancellationToken cancellationToken = default);
    }
}

