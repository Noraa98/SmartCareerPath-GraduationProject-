using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Abstraction.DTOs.RequestDTOs;
using SmartCareerPath.Application.Abstraction.DTOs.ResponseDTOs;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SmartCareerPath.APIs.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterAsync(request);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user {Email}", request?.Email);
                return Problem("An unexpected error occurred while registering the user.", statusCode: 500);
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(request);

                if (result.IsFailure)
                    return Unauthorized(new { error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging in user {Email}", request?.Email);
                return Problem("An unexpected error occurred while logging in.", statusCode: 500);
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RefreshTokenAsync(request);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while refreshing token");
                return Problem("An unexpected error occurred while refreshing token.", statusCode: 500);
            }
        }

        /// <summary>
        /// Logout current user (revoke tokens)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _authService.LogoutAsync(userId.Value);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging out user {UserId}", userId);
                return Problem("An unexpected error occurred while logging out.", statusCode: 500);
            }
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _authService.ChangePasswordAsync(userId.Value, request);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Password changed successfully. Please login again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing password for user {UserId}", userId);
                return Problem("An unexpected error occurred while changing the password.", statusCode: 500);
            }
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // For security do not reveal whether the email exists.
                await _authService.ForgotPasswordAsync(request);

                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing forgot password for {Email}", request?.Email);
                // Still return generic OK to avoid enumeration
                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }
        }

        /// <summary>
        /// Reset password using token from email
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.ResetPasswordAsync(request);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Password reset successfully. You can now login with your new password." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resetting password for {Email}", request?.Email);
                return Problem("An unexpected error occurred while resetting the password.", statusCode: 500);
            }
        }

        /// <summary>
        /// Verify email using token from email
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.VerifyEmailAsync(request);

                if (result.IsFailure)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Email verified successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying email for {Email}", request?.Email);
                return Problem("An unexpected error occurred while verifying the email.", statusCode: 500);
            }
        }

        /// <summary>
        /// Get current authenticated user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCurrentUser()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new UserDto
            {
                Id = userId.Value,
                Email = email,
                Role = role
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}

