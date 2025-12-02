using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Infrastructure.Data;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using System.Security;
using BCrypt.Net;

namespace OnlineLibrary.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailSender emailSender)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return Result.Fail("Tên đăng nhập đã tồn tại.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return Result.Fail("Email đã được sử dụng cho tài khoản khác.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword,
                Role = "User", // Mặc định role là User
                IsLocked = false,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = false
            };

            // Tạo OTP 6 chữ số và hạn sử dụng 10 phút
            var otp = new Random().Next(100000, 999999).ToString();
            user.EmailOtpCode = otp;
            user.EmailOtpExpiry = DateTimeOffset.UtcNow.AddMinutes(10);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            var body =
                $"Xin chào {dto.Username},\n\n" +
                $"Mã xác thực đăng ký thư viện online của bạn là: {otp}.\n" +
                $"Mã có hiệu lực trong 10 phút.\n\n" +
                $"Thân mến,\nOnline Library";
            await _emailSender.SendEmailAsync(dto.Email, "Mã xác thực đăng ký thư viện", body);

            return Result.Ok();

            //var token = GenerateJwtToken(user);
            //var response = new AuthResponseDto(token, user.Username, user.Role);

            //return Result<AuthResponseDto>.Ok(response);
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return Result<AuthResponseDto>.Fail("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            if (!user.EmailConfirmed)
            {
                return Result<AuthResponseDto>.Fail("Email của bạn chưa được xác thực. Vui lòng kiểm tra email để nhập mã OTP.");
            }

            if (user.IsLocked)
            {
                return Result<AuthResponseDto>.Fail("Tài khoản của bạn đã bị khoá.");
            }

            var token = GenerateJwtToken(user);
            var response = new AuthResponseDto(token, user.Username, user.Role);

            return Result<AuthResponseDto>.Ok(response);
        }

        public async Task<Result> VerifyEmailAsync(VerifyEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Result.Fail("Không tìm thấy tài khoản.");

            if (user.EmailConfirmed) return Result.Ok(); // Tài khoản đã xác thực Email

            if (user.EmailOtpCode == null || user.EmailOtpExpiry == null || user.EmailOtpExpiry < DateTimeOffset.UtcNow)
            {
                return Result.Fail("Mã OTP đã hết hạn. Vui lòng đăng nhập và yêu cầu gửi lại.");
            }

            if (!string.Equals(user.EmailOtpCode, dto.OtpCode, StringComparison.Ordinal))
            {
                return Result.Fail("Mã OTP không đúng.");
            }

            user.EmailConfirmed = true;
            user.EmailOtpCode = null;
            user.EmailOtpExpiry = null;

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> SendPasswordResetOtpAsync(ForgotPasswordDto dto)
        {
            // Cho phép nhập username hoặc email
            var input = dto.UsernameOrEmail.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == input ||
                                     (u.Email != null && u.Email.ToLower() == input.ToLower()));

            // Để tránh lộ thông tin tồn tại tài khoản, luôn trả Ok
            if (user == null)
            {
                // Ở môi trường production nên chỉ trả "Nếu tài khoản tồn tại, email sẽ được gửi"
                return Result.Ok();
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return Result.Fail("Tài khoản này chưa có email, không thể đặt lại mật khẩu.");
            }

            if (!user.EmailConfirmed)
            {
                return Result.Fail("Email của tài khoản này chưa được xác thực.");
            }

            // Tạo OTP reset mật khẩu 6 chữ số, hết hạn sau 10 phút
            var otp = new Random().Next(100000, 999999).ToString();
            user.PasswordResetOtpCode = otp;
            user.PasswordResetOtpExpiry = DateTimeOffset.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            var body =
                $"Xin chào {user.Username},\n\n" +
                $"Mã đặt lại mật khẩu thư viện online của bạn là: {otp}.\n" +
                $"Mã có hiệu lực trong 10 phút.\n\n" +
                $"Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.\n\n" +
                $"Thân mến,\nOnline Library";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Mã đặt lại mật khẩu - Online Library",
                body);

            return Result.Ok();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
            {
                return Result.Fail("Không tìm thấy tài khoản.");
            }

            if (user.PasswordResetOtpCode == null || user.PasswordResetOtpExpiry == null ||
                user.PasswordResetOtpExpiry < DateTimeOffset.UtcNow)
            {
                return Result.Fail("Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng yêu cầu lại.");
            }

            if (!string.Equals(user.PasswordResetOtpCode, dto.OtpCode, StringComparison.Ordinal))
            {
                return Result.Fail("Mã OTP không đúng.");
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            {
                return Result.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            // Đặt lại mật khẩu
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Xóa OTP reset để không dùng lại
            user.PasswordResetOtpCode = null;
            user.PasswordResetOtpExpiry = null;

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTimeOffset.UtcNow.AddHours(1).UtcDateTime, // Token hết hạn sau 1 giờ
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
