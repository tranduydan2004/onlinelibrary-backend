using System.Collections.Generic;
namespace OnlineLibrary.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }

        // URL trang bìa (có thể null)
        public string? CoverImageUrl { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties: Một sách có một bản ghi kho
        public BookInventory Inventory { get; set; }

        // Navigation properties: Một sách có thể có nhiều yêu cầu mượn
        public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
    }
}
