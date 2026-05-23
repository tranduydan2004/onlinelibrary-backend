using System;

namespace OnlineLibrary.Domain.Entities
{
    public static class LoanRequestStatus
    {
        public const string Pending = "Đang chờ duyệt";
        public const string Borrowing = "Đang mượn";
        public const string DueSoon = "Sắp đến hạn";
        public const string Overdue = "Quá hạn";
        public const string Returned = "Đã trả";
        public const string Rejected = "Bị từ chối";
    }

    public class LoanRequest
    {
        public int Id { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? DueDate { get; set; } // Ngày hết hạn, có thể null ban đầu
        public string Status { get; set; } // Dùng LoanRequestStatus
        public DateTimeOffset? ReturnDate { get; set; } // Ngày trả thực tế
        public decimal FineAmount { get; set; } = 0; // Tiền phạt

        // Foreign Key đến bảng User
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign Key đến bảng Book
        public int BookId { get; set; }
        public Book Book { get; set; }

        // Số lần đã gia hạn
        public int ExtensionCount { get; set; } = 0;
    }
}
