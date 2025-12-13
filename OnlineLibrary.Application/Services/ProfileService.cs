using OnlineLibrary.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineLibrary.Infrastructure.Data;
using OnlineLibrary.Application.DTOs;

namespace OnlineLibrary.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        public readonly IEmailSender _emailSender;

        public ProfileService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserProfileDto(
                user.Id,
                user.Username,
                user.Role,
                user.Email,
                user.PhoneNumber,
                user.EmailConfirmed
             );
        }

        public async Task<ProfileUpdateResult> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ProfileUpdateResult
                {
                    NotFound = true,
                    EmailChanged = false,
                    Message = "Không tìm thấy người dùng."
                };
            }

            var emailChanged = !string.Equals(
                user.Email,
                dto.Email,
                StringComparison.OrdinalIgnoreCase
            );

            user.PhoneNumber = dto.PhoneNumber;

            if (emailChanged)
            {
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

            return new ProfileUpdateResult
            {
                NotFound = false,
                EmailChanged = emailChanged,
                Message = emailChanged
                    ? "Cập nhật thành công. Vui lòng kiểm tra email mới để xác thực."
                    : "Cập nhật thành công."
            };
        }
    }
}
