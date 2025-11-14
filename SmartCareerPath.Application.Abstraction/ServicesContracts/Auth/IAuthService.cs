using SmartCareerPath.Application.Abstraction.DTOs.RequestDTOs;
using SmartCareerPath.Application.Abstraction.DTOs.ResponseDTOs;
using SmartCareerPath.Domain.Common.ResultPattern;

namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Auth
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request);
        Task<Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request);
        Task<Result<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request);
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequestDTO request);
        Task<Result> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<Result> ResetPasswordAsync(ResetPasswordRequestDTO request);
        Task<Result> VerifyEmailAsync(VerifyEmailRequestDTO request);
        Task<Result> RevokeTokenAsync(string token);
        Task<Result> LogoutAsync(int userId);
    }
}
