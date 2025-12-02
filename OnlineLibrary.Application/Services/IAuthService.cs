using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> LoginAsync(string username, string password);
        Task<Result> VerifyEmailAsync(VerifyEmailDto dto);
        Task<Result> SendPasswordResetOtpAsync(ForgotPasswordDto dto);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    }
}