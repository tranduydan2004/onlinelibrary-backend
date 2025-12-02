using System.Collections.Generic;
namespace OnlineLibrary.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "User", "Admin"
        public bool IsLocked { get; set; } = false;

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? EmailOtpCode { get; set; }
        public DateTimeOffset? EmailOtpExpiry { get; set; }
        public string? PasswordResetOtpCode { get; set; }
        public DateTimeOffset? PasswordResetOtpExpiry { get; set; }

        // Navigation properties: Một người dùng có thể có nhiều yêu cầu mượn sách
        public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
    }
}
