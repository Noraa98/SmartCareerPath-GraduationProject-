using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartCareerPath.Application.Abstraction.DTOs.Payment;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Payment;

namespace SmartCareerPath.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }



        [HttpPost("create-session")]
        [Authorize]
        [ProducesResponseType(typeof(PaymentSessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePaymentSession(
            [FromBody] CreatePaymentSessionRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("=== CreatePaymentSession Request Started ===");

            // Log incoming request body
            if (request != null)
            {
                _logger.LogInformation(
                    "Request payload: UserId={UserId}, ProductType={ProductType}, PaymentProvider={PaymentProvider}, Currency={Currency}, BillingCycle={BillingCycle}, SuccessUrl={SuccessUrl}, CancelUrl={CancelUrl}",
                    request.UserId, request.ProductType, request.PaymentProvider, request.Currency,
                    request.BillingCycle, request.SuccessUrl, request.CancelUrl);
            }

            // Log model state validation errors
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");
                var validationErrors = new Dictionary<string, List<string>>();

                foreach (var kvp in ModelState)
                {
                    var fieldName = kvp.Key;
                    var modelState = kvp.Value;
                    
                    foreach (var error in modelState.Errors)
                    {
                        var errorMessage = error.ErrorMessage;
                        
                        _logger.LogWarning("Validation error - Field: {field}, Message: {message}", fieldName, errorMessage);

                        if (!validationErrors.ContainsKey(fieldName))
                        {
                            validationErrors[fieldName] = new List<string>();
                        }
                        validationErrors[fieldName].Add(errorMessage);
                    }
                }

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = validationErrors,
                    message = "One or more validation errors occurred"
                });
            }

            // Log Authorization header for debugging
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            _logger.LogInformation("Authorization header present: {hasAuth}", !string.IsNullOrEmpty(authHeader));

            string token = null;
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation("Extracted token length: {len}", token.Length);
            }

            // If authenticated, log claims and try to extract user id from token claims
            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is authenticated");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim: {type} = {value}", claim.Type, claim.Value);
                }

                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid");
                if (idClaim != null && int.TryParse(idClaim.Value, out var parsedUserId) && parsedUserId > 0)
                {
                    request.UserId = parsedUserId; // prefer token user id
                    _logger.LogInformation("Using userId from token: {userId}", parsedUserId);
                }
            }
            else
            {
                // No authenticated user - return structured JSON error
                _logger.LogWarning("User not authenticated");
                return Unauthorized(new { error = "Unauthorized", message = "Missing or invalid token" });
            }

            try
            {
                _logger.LogInformation("Calling PaymentService.CreatePaymentSessionAsync with request: UserId={UserId}, ProductType={ProductType}, PaymentProvider={PaymentProvider}",
                    request.UserId, request.ProductType, request.PaymentProvider);

                var result = await _paymentService.CreatePaymentSessionAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogError("Payment service returned failure: {error}", result.Error);
                    return BadRequest(new
                    {
                        error = "Payment session creation failed",
                        details = new { reason = result.Error },
                        message = result.Error
                    });
                }

                _logger.LogInformation("Payment session created successfully. TransactionId: {transactionId}",
                    result.Value?.TransactionId);

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating payment session. Exception type: {exceptionType}, Message: {message}",
                    ex.GetType().Name, ex.Message);

                return BadRequest(new
                {
                    error = "An exception occurred",
                    details = new
                    {
                        exceptionType = ex.GetType().Name,
                        message = ex.Message,
                        stackTrace = ex.StackTrace
                    },
                    message = "Payment session creation failed due to an unexpected error"
                });
            }
        }



        // Verify Payment (Webhook Handler)
        [HttpPost("verify")]
        [AllowAnonymous] // Webhooks come from external providers
        [ProducesResponseType(typeof(PaymentVerificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyPayment(
            [FromBody] VerifyPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paymentService.VerifyPaymentAsync(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }


        // Get Payment Details
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PaymentTransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }


        // Get User Payment History
        [HttpGet("history/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(PaginatedResponse<PaymentHistoryItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentHistory(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _paymentService.GetUserPaymentHistoryAsync(
                userId, pageNumber, pageSize, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        // Create Refund Request
        [HttpPost("refund-request")]
        [Authorize]
        [ProducesResponseType(typeof(RefundRequestResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRefundRequest(
            [FromBody] CreateRefundRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paymentService.CreateRefundRequestAsync(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return StatusCode(201, result.Value);
        }


        // Get Product Pricing
        [HttpGet("pricing")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ProductPricingResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductPricing(
            [FromQuery] int currency = 1, // Default USD
            CancellationToken cancellationToken = default)
        {
            var result = await _paymentService.GetProductPricingAsync(currency, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }


        // Health Check
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Payment Service",
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
