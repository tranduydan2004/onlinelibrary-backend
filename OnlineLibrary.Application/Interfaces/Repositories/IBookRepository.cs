using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Application.Interfaces.Repositories
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdWithInventoryAsync(int id);
        Task<(List<Book> Items, int TotalCount)> SearchBooksAsync(string? keyword, int pageNumber, int pageSize);
        Task<(List<(string Name, int Count)> Items, int TotalCount)> GetGenresAsync(string? search, int pageNumber, int pageSize);
        Task<(List<Book> Items, int TotalCount)> SearchBooksByGenreAsync(string genre, int pageNumber, int pageSize);
        Task AddBookAsync(Book book, BookInventory inventory);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(Book book);
        Task<(List<Book> Items, int TotalCount)> GetAllBooksAsync(int pageNumber, int pageSize);
        Task<List<Book>> GetOutOfStockBooksAsync();
        Task<List<Book>> GetTopQuantityBooksAsync(int topN);
        Task<int> GetTotalBooksCountAsync();
        Task<int> GetBooksAddedSinceAsync(DateTimeOffset since);
        Task<int> GetBooksAddedOnDateAsync(DateTimeOffset date);
    }
}
