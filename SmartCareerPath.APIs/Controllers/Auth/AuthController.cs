using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Abstraction.DTOs.RequestDTOs;
using SmartCareerPath.Application.Abstraction.DTOs.ResponseDTOs;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using System.Security.Claims;

namespace SmartCareerPath.APIs.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        
        // Register a new user
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

       
        // Login with email and password
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (result.IsFailure)
                return Unauthorized(new { error = result.Error });

            return Ok(result.Value);
        }

        
        // Refresh access token using refresh token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

   
        // Logout current user (revoke tokens)
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var result = await _authService.LogoutAsync(userId);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Logged out successfully" });
        }

       
        // Change password for authenticated user
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Password changed successfully. Please login again." });
        }

        
        // Request password reset email
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authService.ForgotPasswordAsync(request);

            // Always return success to prevent email enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        
        // Reset password using token from email
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Password reset successfully. You can now login with your new password." });
        }

       
        // Verify email using token from email
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyEmailAsync(request);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Email verified successfully!" });
        }

        
        // Get current authenticated user info
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(new UserDto
            {
                Id = int.Parse(userId),
                Email = email,
                Role = role
            });
        }

        
        // Health check endpoint
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}

