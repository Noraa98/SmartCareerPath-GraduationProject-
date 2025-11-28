using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _paymentService.CreatePaymentSessionAsync(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
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
