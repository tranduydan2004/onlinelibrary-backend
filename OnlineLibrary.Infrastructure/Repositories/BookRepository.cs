using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Infrastructure.Data;

namespace OnlineLibrary.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Book?> GetByIdWithInventoryAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<(List<Book> Items, int TotalCount)> SearchBooksAsync(string? keyword, int pageNumber, int pageSize)
        {
            IQueryable<Book> query = _context.Books.Include(b => b.Inventory);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword.Trim()}%";
                query = query.Where(b =>
                    EF.Functions.ILike((b.Title ?? string.Empty), pattern) ||
                    EF.Functions.ILike((b.Author ?? string.Empty), pattern));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<(string Name, int Count)> Items, int TotalCount)> GetGenresAsync(string? search, int pageNumber, int pageSize)
        {
            var grouped = await _context.Books
                .AsNoTracking()
                .Where(b => !string.IsNullOrEmpty(b.Genre))
                .GroupBy(b => b.Genre!)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                grouped = grouped.Where(g => g.Name.ToLower().Contains(term)).ToList();
            }

            grouped = grouped.OrderBy(g => g.Name).ToList();

            var totalCount = grouped.Count;
            var skip = (pageNumber - 1) * pageSize;

            var items = grouped
                .Skip(skip)
                .Take(pageSize)
                .Select(g => (g.Name, g.Count))
                .ToList();

            return (items, totalCount);
        }

        public async Task<(List<Book> Items, int TotalCount)> SearchBooksByGenreAsync(string genre, int pageNumber, int pageSize)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Inventory)
                .Where(b => b.Genre == genre);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddBookAsync(Book book, BookInventory inventory)
        {
            _context.Books.Add(book);
            _context.BookInventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(Book book)
        {
            if (book.Inventory != null)
            {
                _context.BookInventories.Remove(book.Inventory);
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Book> Items, int TotalCount)> GetAllBooksAsync(int pageNumber, int pageSize)
        {
            var query = _context.Books.Include(b => b.Inventory);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Book>> GetOutOfStockBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Inventory)
                .Where(b => b.Inventory != null && b.Inventory.Quantity == 0)
                .OrderBy(b => b.Title)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Book>> GetTopQuantityBooksAsync(int topN)
        {
            if (topN <= 0) topN = 10;

            return await _context.Books
                .Include(b => b.Inventory)
                .OrderByDescending(b => b.Inventory != null ? b.Inventory.Quantity : 0)
                .ThenBy(b => b.Title)
                .Take(topN)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.CountAsync();
        }

        public async Task<int> GetBooksAddedSinceAsync(DateTimeOffset since)
        {
            return await _context.Books.CountAsync(b => b.CreatedAt >= since);
        }

        public async Task<int> GetBooksAddedOnDateAsync(DateTimeOffset date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);
            return await _context.Books.CountAsync(b => b.CreatedAt >= startOfDay && b.CreatedAt < endOfDay);
        }
    }
}
