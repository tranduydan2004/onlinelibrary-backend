namespace OnlineLibrary.Domain.Entities
{
    public class BookInventory
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } // "Có sẵn", "Hết sách", ...

        // Foreign Key đến bảng Book
        public int BookId { get; set; }
        // Navigation property (mối quan hệ 1-1)
        public Book Book { get; set; }
    }
}
