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

        // Navigation properties: Một người dùng có thể có nhiều yêu cầu mượn sách
        public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
    }
}
