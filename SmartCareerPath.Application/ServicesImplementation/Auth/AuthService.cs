using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Abstraction.DTOs.RequestDTOs;
using SmartCareerPath.Application.Abstraction.DTOs.ResponseDTOs;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using SmartCareerPath.Domain.Common.ResultPattern;
using SmartCareerPath.Domain.Contracts;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using SmartCareerPath.Infrastructure.Persistence.Data;
using System.Security.Cryptography;
using System.Text;

namespace SmartCareerPath.Application.ServicesImplementation.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ITokenService tokenService,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _context = context;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public async Task<Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result<AuthResponseDTO>.Failure("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Result<AuthResponseDTO>.Failure("Password is required");

            if (request.Password != request.ConfirmPassword)
                return Result<AuthResponseDTO>.Failure("Passwords do not match");

            // Check password strength
            if (!_passwordService.IsStrongPassword(request.Password))
                return Result<AuthResponseDTO>.Failure(
                    "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character");

            // Check if user already exists
            var existingUser = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
                return Result<AuthResponseDTO>.Failure("Email already registered");

            // Get role
            var role = await _unitOfWork.Roles
                .FirstOrDefaultAsync(r => r.Name == request.RoleName);

            if (role == null)
                return Result<AuthResponseDTO>.Failure("Invalid role");

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create user
                // Use combined hash format so VerifyPassword can extract salt correctly
                var combinedHash = _passwordService.HashPassword(request.Password);

                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = combinedHash,
                    // keep PasswordSalt value non-null for DB schema compatibility
                    PasswordSalt = _passwordService.GenerateSalt(),
                    FullName = request.FullName,
                    Phone = request.Phone,
                    RoleId = role?.Id
                };


                // Generate email verification token
                user.EmailVerificationToken = await _tokenService.GenerateEmailVerificationTokenAsync(user.Email);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Create user profile
                var profile = new UserProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Bio = "",
                    CurrentRole = "",           
                    ProfilePictureUrl = "",     
                    LinkedInUrl = "",           
                    GithubUrl = "",             
                    PortfolioUrl = "",          
                    City = "",
                    Country = ""
                };

                await _context.UserProfiles.AddAsync(profile);
                await _context.SaveChangesAsync();

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role.Name);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token, store hash of access token to avoid truncation and for security
                var authToken = new AuthToken
                {
                    UserId = user.Id,
                    Token = ComputeSha256Hash(accessToken),
                    RefreshToken = refreshToken,
                    ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                    RefreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration(),
                    DeviceInfo = "",
                    IpAddress = "",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<AuthToken>().AddAsync(authToken);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // TODO: Send verification email

                return Result<AuthResponseDTO>.Success(new AuthResponseDTO
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = role.Name,
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = authToken.ExpiresAt,
                    RefreshTokenExpiration = authToken.RefreshTokenExpiresAt.Value,
                    IsEmailVerified = user.IsEmailVerified
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<AuthResponseDTO>.Failure($"Registration failed: {ex.Message}");

            }
        }

        public async Task<Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Result<AuthResponseDTO>.Failure("Email and password are required");

            // Find user
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
                return Result<AuthResponseDTO>.Failure("Invalid email or password");

            // Check if account is locked
            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
                return Result<AuthResponseDTO>.Failure($"Account is locked until {user.LockedUntil.Value}");

            // Check if account is active
            if (!user.IsActive)
                return Result<AuthResponseDTO>.Failure("Account is deactivated");

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }

                await _context.SaveChangesAsync();
                return Result<AuthResponseDTO>.Failure("Invalid email or password");
            }

            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastActivity = DateTime.UtcNow;

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.Name);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old tokens
            var oldTokens = await _context.AuthTokens
                .Where(t => t.UserId == user.Id && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in oldTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            // Save new token (store hash)
            var authToken = new AuthToken
            {
                UserId = user.Id,
                Token = ComputeSha256Hash(accessToken),
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                RefreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration(),
                DeviceInfo = "",
                IpAddress = "",
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuthTokens.AddAsync(authToken);
            await _context.SaveChangesAsync();

            return Result<AuthResponseDTO>.Success(new AuthResponseDTO
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.Name,
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = authToken.ExpiresAt,
                RefreshTokenExpiration = authToken.RefreshTokenExpiresAt.Value,
                IsEmailVerified = user.IsEmailVerified
            });
        }

        public async Task<Result<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Result<AuthResponseDTO>.Failure("Refresh token is required");

            // Find token
            var authToken = await _context.AuthTokens
                .Include(t => t.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.RefreshToken == request.RefreshToken && !t.IsRevoked);

            if (authToken == null)
                return Result<AuthResponseDTO>.Failure("Invalid refresh token");

            // Check if refresh token is expired
            if (authToken.RefreshTokenExpiresAt < DateTime.UtcNow)
                return Result<AuthResponseDTO>.Failure("Refresh token expired");

            var user = authToken.User;

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.Name);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old token
            authToken.IsRevoked = true;
            authToken.RevokedAt = DateTime.UtcNow;

            // Save new token (store hash)
            var newAuthToken = new AuthToken
            {
                UserId = user.Id,
                Token = ComputeSha256Hash(newAccessToken),
                RefreshToken = newRefreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                RefreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration(),
                DeviceInfo = "",
                IpAddress = "",
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuthTokens.AddAsync(newAuthToken);
            await _context.SaveChangesAsync();

            return Result<AuthResponseDTO>.Success(new AuthResponseDTO
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.Name,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = newAuthToken.ExpiresAt,
                RefreshTokenExpiration = newAuthToken.RefreshTokenExpiresAt.Value,
                IsEmailVerified = user.IsEmailVerified
            });
        }

        public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequestDTO request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                return Result.Failure("Passwords do not match");

            if (!_passwordService.IsStrongPassword(request.NewPassword))
                return Result.Failure("Password is not strong enough");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result.Failure("User not found");

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return Result.Failure("Current password is incorrect");

            // Update password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Revoke all tokens (force re-login)
            var tokens = await _context.AuthTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequestDTO request)
        {
            var user = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
                return Result.Success(); // Don't reveal if email exists

            // Generate reset token
            var resetToken = await _tokenService.GeneratePasswordResetTokenAsync(user.Email);

            // TODO: Send reset email with token

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                return Result.Failure("Passwords do not match");

            if (!_passwordService.IsStrongPassword(request.NewPassword))
                return Result.Failure("Password is not strong enough");

            // Validate token
            if (!_tokenService.ValidateToken(request.Token))
                return Result.Failure("Invalid or expired token");

            var user = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
                return Result.Failure("User not found");

            // Update password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> VerifyEmailAsync(VerifyEmailRequestDTO request)
        {
            if (!_tokenService.ValidateToken(request.Token))
                return Result.Failure("Invalid or expired token");

            var user = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
                return Result.Failure("User not found");

            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> RevokeTokenAsync(string token)
        {
            var hash = ComputeSha256Hash(token);

            var authToken = await _context.AuthTokens
                .FirstOrDefaultAsync(t => t.Token == hash);

            if (authToken == null)
                return Result.Failure("Token not found");

            authToken.IsRevoked = true;
            authToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> LogoutAsync(int userId)
        {
            var tokens = await _context.AuthTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
