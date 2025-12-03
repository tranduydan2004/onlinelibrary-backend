using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập để truy cập hồ sơ cá nhân
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OnlineLibrary.Application.Common.IEmailSender _emailSender;

        public ProfileController(ApplicationDbContext context, OnlineLibrary.Application.Common.IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Lấy thông tin hồ sơ cá nhân của người dùng hiện tại
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var dto = new UserProfileDto(
                user.Id,
                user.Username,
                user.Role,
                user.Email,
                user.PhoneNumber,
                user.EmailConfirmed
            );

            return Ok(dto);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var emailChanged = !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);
            user.PhoneNumber = dto.PhoneNumber;

            if (emailChanged)
            {
                // Nếu đổi email thì cập nhật & gửi OTP mới
                user.Email = dto.Email;
                user.EmailConfirmed = false;

                var otp = new Random().Next(100000, 999999).ToString();
                user.EmailOtpCode = otp;
                user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);

                var body =
                    $"Xin chào {user.Username},\n\n" +
                    $"Mã xác thực email mới của bạn là: {otp}.\n" +
                    $"Mã có hiệu lực trong 10 phút.\n\n" +
                    $"Thân mến,\nOnline Library";

                await _emailSender.SendEmailAsync(dto.Email, "Mã xác thực email mới", body);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = emailChanged
                    ? "Cập nhật thành công. Vui lòng kiểm tra email mới để xác thực."
                    : "Cập nhật thành công."
            });
        }
    }
}
