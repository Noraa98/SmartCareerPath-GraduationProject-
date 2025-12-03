using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Payment;
using SmartCareerPath.Domain.Enums;
using System.Text.Json;

namespace SmartCareerPath.Application.Strategies.Payment
{
    public class StripePaymentStrategy : IPaymentStrategy
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentStrategy> _logger;
        private readonly string _apiKey;
        private readonly string _webhookSecret;

        public PaymentProvider Provider => PaymentProvider.Stripe;

        public StripePaymentStrategy(
            IConfiguration configuration,
            ILogger<StripePaymentStrategy> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load from appsettings or environment variables
            _apiKey = _configuration["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe API key not configured");
            _webhookSecret = _configuration["Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe webhook secret not configured");
        }

        // ═══════════════════════════════════════════════════════════════
        // Create Payment Session
        // ═══════════════════════════════════════════════════════════════

        public async Task<PaymentSessionResult> CreatePaymentSessionAsync(
            CreatePaymentSessionParams sessionParams,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Creating Stripe checkout session for user {UserId}, amount {Amount} {Currency}",
                    sessionParams.UserId, sessionParams.Amount, sessionParams.Currency);

                // Log all incoming parameters
                _logger.LogInformation("SuccessUrl from params: {SuccessUrl}", sessionParams.SuccessUrl);
                _logger.LogInformation("CancelUrl from params: {CancelUrl}", sessionParams.CancelUrl);

                // Validate that URLs are present
                if (string.IsNullOrWhiteSpace(sessionParams.SuccessUrl))
                {
                    _logger.LogWarning("SuccessUrl is NULL or empty from frontend request");
                    return new PaymentSessionResult
                    {
                        Success = false,
                        ErrorMessage = "SuccessUrl is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(sessionParams.CancelUrl))
                {
                    _logger.LogWarning("CancelUrl is NULL or empty from frontend request");
                    return new PaymentSessionResult
                    {
                        Success = false,
                        ErrorMessage = "CancelUrl is required"
                    };
                }

                // Ensure URLs start with http:// or https://
                var successUrl = sessionParams.SuccessUrl.Trim();
                var cancelUrl = sessionParams.CancelUrl.Trim();

                if (!successUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !successUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    successUrl = "http://localhost:4200" + (successUrl.StartsWith("/") ? "" : "/") + successUrl;
                    _logger.LogInformation("SuccessUrl adjusted to: {AdjustedUrl}", successUrl);
                }

                if (!cancelUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !cancelUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    cancelUrl = "http://localhost:4200" + (cancelUrl.StartsWith("/") ? "" : "/") + cancelUrl;
                    _logger.LogInformation("CancelUrl adjusted to: {AdjustedUrl}", cancelUrl);
                }

                // IMPORTANT: Replace the success URL with our backend redirect endpoint
                // This endpoint will auto-redirect to the Angular payment-success component
                // Extract the base URL from successUrl (before query params) and use our redirect endpoint
                var successUrlBase = successUrl.Split('?')[0];
                var successUrlHost = new System.Uri(successUrlBase).GetLeftPart(System.UriPartial.Authority);
                var modifiedSuccessUrl = $"{successUrlHost}/api/payment/stripe/redirect?session_id={{CHECKOUT_SESSION_ID}}";
                
                _logger.LogInformation("Modified SuccessUrl from: {Original} to: {Modified}", successUrl, modifiedSuccessUrl);

                // Set Stripe API key
                Stripe.StripeConfiguration.ApiKey = _apiKey;

                // Determine the mode (subscription or payment)
                var mode = GetStripeMode(sessionParams.BillingCycle);
                _logger.LogInformation("Stripe session mode: {Mode}", mode);

                // Build PriceData with Recurring if in subscription mode
                var priceDataOptions = new Stripe.Checkout.SessionLineItemPriceDataOptions
                {
                    Currency = sessionParams.Currency.ToString().ToLower(),
                    ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                    {
                        Name = GetProductName(sessionParams.ProductType),
                        Description = GetProductDescription(sessionParams.ProductType)
                    },
                    UnitAmount = (long)(sessionParams.Amount * 100),
                };

                // Add recurring interval if subscription mode
                if (mode == "subscription")
                {
                    var interval = (sessionParams.BillingCycle == BillingCycle.Yearly) ? "year" : "month";
                    priceDataOptions.Recurring = new Stripe.Checkout.SessionLineItemPriceDataRecurringOptions
                    {
                        Interval = interval
                    };
                    _logger.LogInformation("Added recurring interval: {Interval}", interval);
                }

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            PriceData = priceDataOptions,
                            Quantity = 1,
                        },
                    },
                    Mode = mode,
                    SuccessUrl = modifiedSuccessUrl,
                    CancelUrl = cancelUrl,
                    CustomerEmail = sessionParams.CustomerEmail,
                    ClientReferenceId = sessionParams.UserId.ToString(),
                    Metadata = sessionParams.Metadata
                };

                _logger.LogInformation("Creating Stripe session with SuccessUrl: {SuccessUrl}, CancelUrl: {CancelUrl}", modifiedSuccessUrl, cancelUrl);

                var service = new Stripe.Checkout.SessionService();
                var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

                _logger.LogInformation("Stripe session created: {SessionId}, URL: {SessionUrl}", session.Id, session.Url);
                // Log what Stripe reports for the success/cancel URLs (helps debug redirect issues)
                try
                {
                    _logger.LogInformation("Stripe returned SuccessUrl: {SuccessUrl}", session.SuccessUrl);
                    _logger.LogInformation("Stripe returned CancelUrl: {CancelUrl}", session.CancelUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log Stripe session SuccessUrl/CancelUrl");
                }

                return new PaymentSessionResult
                {
                    Success = true,
                    ProviderReference = session.Id,
                    CheckoutUrl = session.Url,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    ProviderMetadata = new Dictionary<string, string>
                {
                    { "session_id", session.Id },
                    { "mode", GetStripeMode(sessionParams.BillingCycle) }
                }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe checkout session");
                return new PaymentSessionResult
                {
                    Success = false,
                    ErrorMessage = $"Stripe session creation failed: {ex.Message}"
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Webhook Verification
        // ═══════════════════════════════════════════════════════════════

        public bool VerifyWebhookSignature(string payload, string signature, string secret)
        {
            try
            {
                // TODO: Use Stripe SDK for actual signature verification
                /*
                var stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    secret
                );
                return true;
                */

                // Mock verification for development
                _logger.LogInformation("Verifying Stripe webhook signature");
                return !string.IsNullOrEmpty(signature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Parse Webhook Payload
        // ═══════════════════════════════════════════════════════════════

        public WebhookPaymentInfo ParseWebhookPayload(string payload)
        {
            try
            {
                // TODO: Use Stripe SDK for actual parsing
                /*
                var stripeEvent = EventUtility.ParseEvent(payload);

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    return MapStripeSessionToPaymentInfo(session);
                }
                */

                // Mock parsing for development
                var mockData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);

                return new WebhookPaymentInfo
                {
                    ProviderReference = mockData?["session_id"].GetString() ?? "",
                    Status = PaymentStatus.Completed,
                    Amount = mockData?["amount"].GetDecimal() ?? 0,
                    Currency = Enum.Parse<Currency>(mockData?["currency"].GetString() ?? "USD", true),
                    Metadata = new Dictionary<string, string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Stripe webhook payload");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Get Payment Status
        // ═══════════════════════════════════════════════════════════════

        public async Task<ProviderPaymentStatus> GetPaymentStatusAsync(
            string providerReference,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching payment status from Stripe for {Reference}", providerReference);

                // TODO: Use Stripe SDK
                /*
                var service = new SessionService();
                var session = await service.GetAsync(providerReference, cancellationToken: cancellationToken);
                return MapStripeSessionToStatus(session);
                */

                // Mock response
                return new ProviderPaymentStatus
                {
                    Status = PaymentStatus.Completed,
                    Amount = 9.99m,
                    Currency = Currency.USD,
                    CompletedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payment status from Stripe");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Process Refund
        // ═══════════════════════════════════════════════════════════════

        public async Task<RefundResult> ProcessRefundAsync(
            string providerReference,
            decimal amount,
            Currency currency,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Processing Stripe refund for {Reference}, amount {Amount}",
                    providerReference, amount);

                // TODO: Use Stripe SDK
                /*
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = providerReference,
                    Amount = (long)(amount * 100),
                    Reason = MapRefundReason(reason)
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);
                */

                // Mock response
                return new RefundResult
                {
                    Success = true,
                    RefundReference = $"re_test_{Guid.NewGuid().ToString("N")[..24]}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Stripe refund");
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = $"Refund failed: {ex.Message}"
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Helper Methods
        // ═══════════════════════════════════════════════════════════════

        private string GetStripeMode(BillingCycle? billingCycle)
        {
            return billingCycle switch
            {
                BillingCycle.Monthly or BillingCycle.Yearly => "subscription",
                BillingCycle.Lifetime or BillingCycle.PayPerUse => "payment",
                _ => "payment"
            };
        }

        private string GetProductName(ProductType productType)
        {
            return productType switch
            {
                ProductType.InterviewerSubscription => "AI Interviewer Pro",
                ProductType.CVBuilderSubscription => "Smart CV Builder",
                ProductType.BundleSubscription => "Career Pro Bundle",
                ProductType.InterviewerLifetime => "AI Interviewer - Lifetime",
                ProductType.CVBuilderLifetime => "CV Builder - Lifetime",
                ProductType.BundleLifetime => "Career Bundle - Lifetime",
                ProductType.SingleInterview => "Single Interview Session",
                ProductType.SingleCV => "Single CV Generation",
                _ => "Smart Career Path Product"
            };
        }

        private string GetProductDescription(ProductType productType)
        {
            return productType switch
            {
                ProductType.InterviewerSubscription => "Unlimited AI-powered interview practice",
                ProductType.CVBuilderSubscription => "Professional CV builder with AI assistance",
                ProductType.BundleSubscription => "Complete career toolkit - Interview + CV",
                _ => "Smart Career Path Service"
            };
        }
    }
}
