using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string email, string role);
        string GenerateRefreshToken();
        Task<string> GenerateEmailVerificationTokenAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);

        // Optional: Methods to get token expiration dates
        DateTime GetAccessTokenExpiration();
        DateTime GetRefreshTokenExpiration();
        DateTime? GetTokenExpirationDate(string token);
        ClaimsPrincipal GetPrincipalFromToken(string token);

    }

}
