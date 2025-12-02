using System;
namespace OnlineLibrary.Domain.Entities
{
    public class LoanRequest
    {
        public int Id { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? DueDate { get; set; } // Ngày hết hạn, có thể null ban đầu
        public string Status { get; set; } // Trạng thái: "Đang chờ duyệt", "Đang mượn", "Đã trả"

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
