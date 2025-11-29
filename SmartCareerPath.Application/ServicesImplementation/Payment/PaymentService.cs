using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCareerPath.Application.Abstraction.DTOs.Payment;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Payment;
using SmartCareerPath.Application.Strategies.Payment;
using SmartCareerPath.Domain.Common.ResultPattern;
using SmartCareerPath.Domain.Contracts;
using SmartCareerPath.Domain.Entities.Payments;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using SmartCareerPath.Domain.Enums;
using SmartCareerPath.Infrastructure.Persistence.Data;

namespace SmartCareerPath.Application.ServicesImplementation.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentStrategyFactory _strategyFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            PaymentStrategyFactory strategyFactory,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _strategyFactory = strategyFactory;
            _mapper = mapper;
            _logger = logger;
            _logger = logger;
            _configuration = configuration;
        }


        // Create Payment Session
        public async Task<Result<PaymentSessionResponse>> CreatePaymentSessionAsync(
            CreatePaymentSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "=== PaymentService.CreatePaymentSessionAsync Started === UserId={UserId}, ProductType={ProductType}, PaymentProvider={PaymentProvider}, Currency={Currency}, BillingCycle={BillingCycle}",
                    request.UserId, request.ProductType, request.PaymentProvider, request.Currency, request.BillingCycle);

                // 1. Validate user exists
                _logger.LogInformation("Step 1: Validating user exists. UserId={UserId}", request.UserId);
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogError("User not found. UserId={UserId}", request.UserId);
                    return Result<PaymentSessionResponse>.Failure($"User not found with ID {request.UserId}");
                }
                _logger.LogInformation("User found: {UserEmail}", user.Email);

                // 2. Get pricing
                _logger.LogInformation("Step 2: Converting request values to enums");
                var productType = (ProductType)request.ProductType;
                var currency = (Currency)request.Currency;
                var billingCycle = request.BillingCycle.HasValue
                    ? (BillingCycle)request.BillingCycle.Value
                    : BillingCycle.PayPerUse;

                _logger.LogInformation("Converted enums: ProductType={ProductType}, Currency={Currency}, BillingCycle={BillingCycle}",
                    productType, currency, billingCycle);

                decimal amount;
                try
                {
                    _logger.LogInformation("Getting price from PaymentSeeder for ProductType={ProductType}, Currency={Currency}, BillingCycle={BillingCycle}",
                        productType, currency, billingCycle);
                    amount = PaymentSeeder.PricingConfig.GetPrice(productType, currency, billingCycle);
                    _logger.LogInformation("Price retrieved: {Amount} {Currency}", amount, currency);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Failed to get price from PaymentSeeder. ProductType={ProductType}, Currency={Currency}, BillingCycle={BillingCycle}",
                        productType, currency, billingCycle);
                    return Result<PaymentSessionResponse>.Failure($"Pricing error: {ex.Message}");
                }

                // 3. Apply discount if provided
                if (!string.IsNullOrEmpty(request.DiscountCode))
                {
                    _logger.LogInformation("Step 3: Processing discount code: {Code}", request.DiscountCode);
                    // TODO: Implement discount code validation
                }
                else
                {
                    _logger.LogInformation("Step 3: No discount code provided");
                }

                // 4. Get payment strategy
                _logger.LogInformation("Step 4: Getting payment strategy for provider: {Provider}", request.PaymentProvider);
                var provider = (PaymentProvider)request.PaymentProvider;
                IPaymentStrategy strategy;
                try
                {
                    strategy = _strategyFactory.GetStrategy(provider);
                    _logger.LogInformation("Payment strategy obtained for provider: {Provider}", provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get payment strategy for provider: {Provider}", provider);
                    return Result<PaymentSessionResponse>.Failure($"Payment provider error: {ex.Message}");
                }

                // 5. Create payment session with provider
                _logger.LogInformation("Step 5: Creating payment session parameters");
                var sessionParams = new CreatePaymentSessionParams
                {
                    UserId = request.UserId,
                    Amount = amount,
                    Currency = currency,
                    ProductType = productType,
                    BillingCycle = billingCycle,
                    CustomerEmail = user.Email,
                    CustomerName = user.FullName,
                    SuccessUrl = request.SuccessUrl,
                    CancelUrl = request.CancelUrl,
                    Metadata = new Dictionary<string, string>
                {
                    { "user_id", request.UserId.ToString() },
                    { "product_type", productType.ToString() },
                    { "billing_cycle", billingCycle.ToString() }
                }
                };

                _logger.LogInformation("Calling payment provider to create session. Amount={Amount}, CustomerEmail={CustomerEmail}, SuccessUrl={SuccessUrl}, CancelUrl={CancelUrl}",
                    amount, user.Email, request.SuccessUrl, request.CancelUrl);

                var sessionResult = await strategy.CreatePaymentSessionAsync(sessionParams, cancellationToken);

                if (!sessionResult.Success)
                {
                    _logger.LogError("Payment provider session creation failed. ErrorMessage={ErrorMessage}", sessionResult.ErrorMessage);
                    return Result<PaymentSessionResponse>.Failure(
                        sessionResult.ErrorMessage ?? "Failed to create payment session with payment provider");
                }

                _logger.LogInformation("Payment session created with provider. ProviderReference={ProviderReference}, CheckoutUrl={CheckoutUrl}",
                    sessionResult.ProviderReference, sessionResult.CheckoutUrl);

                // 6. Save payment transaction to database
                _logger.LogInformation("Step 6: Saving payment transaction to database");
                var payment = new PaymentTransaction
                {
                    UserId = request.UserId,
                    Provider = provider,
                    ProviderReference = sessionResult.ProviderReference!,
                    Amount = amount,
                    Currency = currency,
                    ProductType = productType,
                    Status = PaymentStatus.Pending,
                    BillingCycle = billingCycle,
                    CheckoutUrl = sessionResult.CheckoutUrl,
                    ExpiresAt = sessionResult.ExpiresAt,
                    ProviderMetadata = System.Text.Json.JsonSerializer.Serialize(sessionResult.ProviderMetadata),
                    DiscountCode = request.DiscountCode
                };

                try
                {
                    await _unitOfWork.Repository<PaymentTransaction>().AddAsync(payment);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Payment transaction saved to database. TransactionId={TransactionId}", payment.Id);
                }
                catch (Exception ex)
                {
                    string innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                    _logger.LogError(ex, "Failed to save payment transaction to database. Exception: {ExceptionMessage}. Inner: {InnerMessage}", ex.Message, innerExceptionMessage);
                    return Result<PaymentSessionResponse>.Failure($"Database error: {ex.Message}");
                }

                // 7. Return response
                var response = new PaymentSessionResponse
                {
                    TransactionId = payment.Id,
                    ProviderReference = payment.ProviderReference,
                    CheckoutUrl = payment.CheckoutUrl!,
                    Amount = payment.Amount,
                    Currency = payment.Currency.ToString(),
                    ProductType = payment.ProductType.ToString(),
                    ExpiresAt = payment.ExpiresAt ?? DateTime.UtcNow.AddHours(24)
                };

                _logger.LogInformation(
                    "=== Payment session created successfully. TransactionId={TransactionId}, CheckoutUrl={CheckoutUrl} ===",
                    payment.Id, payment.CheckoutUrl);

                return Result<PaymentSessionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== EXCEPTION in CreatePaymentSessionAsync. Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace} ===",
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                return Result<PaymentSessionResponse>.Failure($"An error occurred: {ex.Message}");
            }
        }


        // Verify Payment
        public async Task<Result<PaymentVerificationResponse>> VerifyPaymentAsync(VerifyPaymentRequest request,
                     CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Verifying payment for provider reference: {Reference}",
                    request.ProviderReference);

                // 1. Find payment transaction
                var payments = await _unitOfWork.Repository<PaymentTransaction>()
                    .FindAsync(p => p.ProviderReference == request.ProviderReference);

                var paymentTransaction = payments.FirstOrDefault();

                if (paymentTransaction == null)
                {
                    _logger.LogWarning("Payment transaction not found for reference: {Reference}", request.ProviderReference);
                    return Result<PaymentVerificationResponse>.Failure(
                        $"Payment transaction with reference '{request.ProviderReference}' not found. Please create a payment session first.");
                }

                // 2. Check if already processed
                if (paymentTransaction.Status == PaymentStatus.Completed)
                {
                    _logger.LogWarning("Payment {Id} already completed", paymentTransaction.Id);
                    return Result<PaymentVerificationResponse>.Success(new PaymentVerificationResponse
                    {
                        TransactionId = paymentTransaction.Id,
                        Status = "Completed",
                        SubscriptionId = paymentTransaction.SubscriptionId,
                        Message = "Payment already processed",
                        CompletedAt = paymentTransaction.CompletedAt
                    });
                }

                // 3. Get payment strategy
                IPaymentStrategy strategy;
                try
                {
                    strategy = _strategyFactory.GetStrategy(paymentTransaction.Provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get payment strategy for provider {Provider}", paymentTransaction.Provider);
                    return Result<PaymentVerificationResponse>.Failure(
                        $"Payment provider {paymentTransaction.Provider} is not supported");
                }

                // 4. Verify webhook signature if provided
                if (!string.IsNullOrEmpty(request.Signature) && !string.IsNullOrEmpty(request.WebhookPayload))
                {
                    try
                    {
                        var webhookSecret = GetWebhookSecret(paymentTransaction.Provider);

                        if (!strategy.VerifyWebhookSignature(request.WebhookPayload, request.Signature, webhookSecret))
                        {
                            _logger.LogWarning("Invalid webhook signature for payment {Id}", paymentTransaction.Id);
                            return Result<PaymentVerificationResponse>.Failure("Invalid webhook signature");
                        }

                        _logger.LogInformation("Webhook signature verified successfully for payment {Id}", paymentTransaction.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error verifying webhook signature");
                        // Continue anyway for development/testing
                    }
                }

                // 5. Parse webhook payload or fetch status from provider
                WebhookPaymentInfo paymentInfo;

                try
                {
                    if (!string.IsNullOrEmpty(request.WebhookPayload))
                    {
                        _logger.LogInformation("Parsing webhook payload");
                        paymentInfo = strategy.ParseWebhookPayload(request.WebhookPayload);
                    }
                    else
                    {
                        _logger.LogInformation("Fetching payment status from provider");
                        var providerStatus = await strategy.GetPaymentStatusAsync(
                            request.ProviderReference, cancellationToken);

                        paymentInfo = new WebhookPaymentInfo
                        {
                            ProviderReference = request.ProviderReference,
                            Status = providerStatus.Status,
                            Amount = providerStatus.Amount,
                            Currency = providerStatus.Currency,
                            PaymentMethod = providerStatus.PaymentMethod
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse webhook or fetch payment status");
                    return Result<PaymentVerificationResponse>.Failure(
                        $"Failed to verify payment: {ex.Message}");
                }

                // 6. Validate amount matches (important security check)
                if (paymentInfo.Amount != paymentTransaction.Amount)
                {
                    _logger.LogWarning(
                        "Amount mismatch! Expected {Expected}, got {Actual}",
                        paymentTransaction.Amount, paymentInfo.Amount);

                    // Log but don't fail in development mode
                    // In production, you might want to fail here
                }

                // 7. Update payment transaction
                paymentTransaction.Status = paymentInfo.Status;
                paymentTransaction.PaymentMethod = paymentInfo.PaymentMethod;
                paymentTransaction.WebhookPayload = request.WebhookPayload;

                if (paymentInfo.Status == PaymentStatus.Completed)
                {
                    paymentTransaction.MarkAsCompleted();

                    _logger.LogInformation("Payment {Id} marked as completed", paymentTransaction.Id);

                    // 8. Activate subscription
                    try
                    {
                        var subscriptionId = await ActivateSubscriptionAsync(paymentTransaction);
                        paymentTransaction.SubscriptionId = subscriptionId;

                        _logger.LogInformation(
                            "Subscription {SubscriptionId} activated for user {UserId}",
                            subscriptionId, paymentTransaction.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to activate subscription for payment {PaymentId}", paymentTransaction.Id);
                        throw;
                    }

                }
                else if (paymentInfo.Status == PaymentStatus.Failed)
                {
                    paymentTransaction.MarkAsFailed(
                        paymentInfo.ErrorMessage ?? "Payment failed",
                        "provider_error");

                    _logger.LogWarning("Payment {Id} marked as failed", paymentTransaction.Id);
                }

                await _unitOfWork.Repository<PaymentTransaction>().UpdateAsync(paymentTransaction);
                await _unitOfWork.SaveChangesAsync();

                // 9. Return response
                var response = new PaymentVerificationResponse
                {
                    TransactionId = paymentTransaction.Id,
                    Status = paymentTransaction.Status.ToString(),
                    SubscriptionId = paymentTransaction.SubscriptionId,
                    Message = GetVerificationMessage(paymentTransaction.Status),
                    CompletedAt = paymentTransaction.CompletedAt
                };

                _logger.LogInformation(
                    "Payment verification completed successfully. TransactionId: {Id}, Status: {Status}",
                    paymentTransaction.Id, paymentTransaction.Status);

                return Result<PaymentVerificationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying payment for reference {Reference}", request.ProviderReference);
                return Result<PaymentVerificationResponse>.Failure(
                    $"An unexpected error occurred while verifying payment: {ex.Message}");
            }
        }

        // Helper method to get webhook secret
        private string GetWebhookSecret(PaymentProvider provider)
        {
            return provider switch
            {
                PaymentProvider.Stripe => _configuration["Stripe:WebhookSecret"] ?? "",
                PaymentProvider.PayPal => _configuration["PayPal:WebhookId"] ?? "",
                PaymentProvider.Paymob => _configuration["Paymob:HmacSecret"] ?? "",
                _ => ""
            };
        }


        // Helper Methods
        private async Task<int> ActivateSubscriptionAsync(PaymentTransaction payment)
        {
            _logger.LogInformation("Activating subscription for user {UserId}", payment.UserId);

            // Update user role to Premium
            var user = await _unitOfWork.Users.GetByIdAsync(payment.UserId);
            if (user != null)
            {
                var premiumRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Name == "Premium");
                if (premiumRole != null)
                {
                    user.RoleId = premiumRole.Id;
                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} role updated to Premium", payment.UserId);
                }
            }

            var plans = await _unitOfWork.SubscriptionPlans.GetAllAsync();
            SubscriptionPlan defaultPlan;

            if (!plans.Any())
            {
                defaultPlan = new SubscriptionPlan
                {
                    Name = "Default Plan",
                    Description = "This is the default subscription plan", 
                    Price = 0,
                    DurationMonths = 1,
                    Currency = "USD", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    MonthlyAIRequestsLimit = 0,
                    MonthlyJobApplicationsLimit = 0,
                    MonthlyMockInterviewsLimit = 0,
                    MonthlyQuizAttemptsLimit = 0,
                    MonthlyResumeParsingLimit = 0,
                    HasAdvancedAI = false,
                    HasCareerPathAccess = false,
                    HasPremiumTemplates = false,
                    HasPrioritySupport = false
                };

                await _unitOfWork.SubscriptionPlans.AddAsync(defaultPlan);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("No subscription plans found. Created default plan with ID {PlanId}", defaultPlan.Id);
            }

            else
            {
                defaultPlan = plans.First();
            }

            var defaultPlanId = defaultPlan.Id;

            var subscriptionsQuery = await _unitOfWork.UserSubscriptions
                .FindAsync(s => s.UserId == payment.UserId);
            var subscription = subscriptionsQuery.FirstOrDefault();

            if (subscription == null)
            {
                subscription = new UserSubscription
                {
                    UserId = payment.UserId,
                    PlanId = defaultPlanId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(defaultPlan.DurationMonths),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CancellationReason = string.Empty
                };

                await _unitOfWork.UserSubscriptions.AddAsync(subscription);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Created new subscription {SubscriptionId} for user {UserId}", subscription.Id, payment.UserId);
            }
            else
            {
                subscription.EndDate = subscription.EndDate < DateTime.UtcNow
                    ? DateTime.UtcNow.AddMonths(defaultPlan.DurationMonths)
                    : subscription.EndDate.AddMonths(defaultPlan.DurationMonths);
                subscription.IsActive = true;
                subscription.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Extended subscription {SubscriptionId} for user {UserId}", subscription.Id, payment.UserId);
            }

            return subscription.Id;
        }



        private string GetVerificationMessage(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Completed => "Payment completed successfully",
                PaymentStatus.Failed => "Payment failed",
                PaymentStatus.Pending => "Payment is still pending",
                PaymentStatus.Cancelled => "Payment was cancelled",
                _ => "Payment status unknown"
            };
        }

        // Get Payment By ID
        public async Task<Result<PaymentTransactionResponse>> GetPaymentByIdAsync(
            int paymentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _unitOfWork.Repository<PaymentTransaction>().GetByIdAsync(paymentId);

                if (payment == null)
                {
                    return Result<PaymentTransactionResponse>.Failure("Payment not found");
                }

                var response = _mapper.Map<PaymentTransactionResponse>(payment);
                return Result<PaymentTransactionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", paymentId);
                return Result<PaymentTransactionResponse>.Failure("Error retrieving payment");
            }
        }

      
        // Get User Payment History
        public async Task<Result<PaginatedResponse<PaymentHistoryItemResponse>>> GetUserPaymentHistoryAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Calculate skip
                var skip = (pageNumber - 1) * pageSize;

                // Get payments
                var payments = await _unitOfWork.Repository<PaymentTransaction>()
                    .FindAsync(p => p.UserId == userId);

                var orderedPayments = payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                // Map to response
                var items = orderedPayments.Select(p => new PaymentHistoryItemResponse
                {
                    TransactionId = p.Id,
                    ProductType = p.ProductType.ToString(),
                    Amount = p.Amount,
                    Currency = p.Currency.ToString(),
                    DisplayAmount = p.GetDisplayAmount(),
                    Status = p.Status.ToString(),
                    Provider = p.Provider.ToString(),
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt,
                    ReceiptUrl = p.ReceiptUrl
                }).ToList();

                var response = new PaginatedResponse<PaymentHistoryItemResponse>
                {
                    Items = items,
                    TotalItems = payments.Count(),
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Result<PaginatedResponse<PaymentHistoryItemResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for user {UserId}", userId);
                return Result<PaginatedResponse<PaymentHistoryItemResponse>>.Failure("Error retrieving payment history");
            }
        }

        
        // Create Refund Request
        public async Task<Result<RefundRequestResponse>> CreateRefundRequestAsync(
            CreateRefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Creating refund request for payment {PaymentId}",
                    request.PaymentTransactionId);

                // 1. Validate payment exists and can be refunded
                var payment = await _unitOfWork.Repository<PaymentTransaction>()
                    .GetByIdAsync(request.PaymentTransactionId);

                if (payment == null)
                {
                    return Result<RefundRequestResponse>.Failure("Payment not found");
                }

                if (!payment.CanBeRefunded())
                {
                    return Result<RefundRequestResponse>.Failure(
                        "Payment cannot be refunded. Only completed payments can be refunded.");
                }

                if (request.RefundAmount > payment.Amount)
                {
                    return Result<RefundRequestResponse>.Failure(
                        "Refund amount cannot exceed payment amount");
                }

                // 2. Create refund request
                var refundRequest = new RefundRequest
                {
                    PaymentTransactionId = request.PaymentTransactionId,
                    UserId = request.UserId,
                    RefundAmount = request.RefundAmount,
                    Currency = payment.Currency,
                    Reason = request.Reason,
                    Status = RefundStatus.Requested
                };

                await _unitOfWork.Repository<RefundRequest>().AddAsync(refundRequest);
                await _unitOfWork.SaveChangesAsync();

                // 3. Map to response
                var response = _mapper.Map<RefundRequestResponse>(refundRequest);

                _logger.LogInformation("Refund request created: {RefundRequestId}", refundRequest.Id);

                return Result<RefundRequestResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request");
                return Result<RefundRequestResponse>.Failure("Error creating refund request");
            }
        }


        // Get Product Pricing
        public async Task<Result<List<ProductPricingResponse>>> GetProductPricingAsync(
            int currency,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currencyEnum = (Currency)currency;
                // Return a single plan called "Careera Premium" that bundles the AI Interviewer
                // and the Job Description Parser. Frontend will handle icons/styling.
                var products = new List<ProductPricingResponse>();

                // Define base amounts in USD. Frontend/back-end currency conversion can be
                // improved later; for now amounts are shown using the requested currency label.
                decimal monthlyUsd = 9.99m;
                decimal yearlyUsd = 99.99m; // ~17% discount vs monthly * 12
                decimal lifetimeUsd = 199.99m;

                // If needed, you can extend this to convert amounts based on currency.
                var tiers = new List<PricingTierResponse>
                {
                    new PricingTierResponse
                    {
                        BillingCycleId = (int)BillingCycle.Monthly,
                        BillingCycle = BillingCycle.Monthly.ToString(),
                        Amount = monthlyUsd,
                        Currency = currencyEnum.ToString(),
                        DisplayAmount = FormatCurrency(monthlyUsd, currencyEnum),
                        DiscountPercentage = null,
                        DiscountLabel = null
                    },
                    new PricingTierResponse
                    {
                        BillingCycleId = (int)BillingCycle.Yearly,
                        BillingCycle = BillingCycle.Yearly.ToString(),
                        Amount = yearlyUsd,
                        Currency = currencyEnum.ToString(),
                        DisplayAmount = FormatCurrency(yearlyUsd, currencyEnum),
                        DiscountPercentage = 17,
                        DiscountLabel = "Save 17%"
                    },
                    new PricingTierResponse
                    {
                        BillingCycleId = (int)BillingCycle.Lifetime,
                        BillingCycle = BillingCycle.Lifetime.ToString(),
                        Amount = lifetimeUsd,
                        Currency = currencyEnum.ToString(),
                        DisplayAmount = FormatCurrency(lifetimeUsd, currencyEnum),
                        DiscountPercentage = null,
                        DiscountLabel = null
                    }
                };

                var features = new List<string>
                {
                    "Access to AI Interviewer (realistic mock interviews)",
                    "Job Description Parser — analyze JD and match to your CV",
                    "Unlimited interview sessions and parsing requests",
                    "Detailed AI feedback and downloadable reports",
                    "Priority support and continuous updates",
                    "Access to advanced scoring and performance insights"
                };

                products.Add(new ProductPricingResponse
                {
                    ProductTypeId = (int)ProductType.BundleSubscription,
                    ProductType = "CareeraPremium",
                    DisplayName = "Careera Premium",
                    Description = "All-in-one premium subscription that includes unlimited access to the AI Interviewer and an advanced Job Description Parser. Improve your interview readiness with realistic mock interviews, get personalized feedback, and quickly analyze job descriptions to tailor your CV and applications.",
                    Tiers = tiers,
                    Features = features
                });

                return await Task.FromResult(Result<List<ProductPricingResponse>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product pricing");
                return Result<List<ProductPricingResponse>>.Failure("Error retrieving pricing");
            }
        }

        // Handle Webhook Event
        public async Task<Result> HandleWebhookEventAsync(
            int provider,
            string payload,
            string signature,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Handling webhook event from provider {Provider}", provider);

                // TODO: Implement webhook event handling
                // This should validate the signature and process the webhook payload

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook event");
                return Result.Failure("An error occurred while handling webhook event");
            }
        }

        // Helper Methods
        private string FormatCurrency(decimal amount, Currency currency)
        {
            return currency switch
            {
                Currency.USD => $"${amount:F2}",
                Currency.EUR => $"€{amount:F2}",
                Currency.GBP => $"£{amount:F2}",
                Currency.EGP => $"{amount:F2} EGP",
                Currency.SAR => $"{amount:F2} SAR",
                _ => $"{amount:F2} {currency}"
            };
        }
    }
}
