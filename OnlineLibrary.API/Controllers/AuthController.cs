using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Services;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new
            {
                message = "Đăng ký thành công! Vui lòng kiểm tra email để nhập mã OTP xác thực tài khoản."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto.Username, dto.Password);
            if ( !result.IsSuccess)
            {
                return Unauthorized(new { error = result.Error });
            }
            return Ok(result.Value);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            var result = await _authService.VerifyEmailAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(new { message = "Xác thực email thành công. Bạn có thể đăng nhập." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await _authService.SendPasswordResetOtpAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            // Có thể luôn trả mesage chung chung
            return Ok(new { message = "Nếu tài khoản tồn tại, mã đặt lại mật khẩu đã được gửi vào email đăng ký tài khoản." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới." });
        }
    }
}
