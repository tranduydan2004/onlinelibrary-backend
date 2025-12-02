using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Infrastructure.Data;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Application.Extensions;

namespace OnlineLibrary.Application.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<BookDto>> GetBookByIdAsync(int id)
        {
            var bookDto = await _context.Books
                .Include(b => b.Inventory)
                .Where(b => b.Id == id)
                .Select(b => new BookDto(
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Genre,
                    b.Inventory != null ? b.Inventory.Quantity : 0,
                    b.Inventory != null ? b.Inventory.Status : "Không rõ",
                    b.CoverImageUrl
                 ))
                .FirstOrDefaultAsync();

            if (bookDto == null)
            {
                return Result<BookDto>.Fail("Không tìm thấy sách.");
            }

            return Result<BookDto>.Ok(bookDto);
        }

        public async Task<PagedResult<BookDto>> SearchBooksAsync(string? keyword, int pageNumber, int pageSize)
        {
            IQueryable<Book> query = _context.Books;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword.Trim()}%"; // PostgreSQL ILIKE pattern
                query = query.Where(b =>
                    EF.Functions.ILike((b.Title ?? string.Empty), pattern) ||
                    EF.Functions.ILike((b.Author ?? string.Empty), pattern));
            }

            var dtoQuery = query
                .Include(b => b.Inventory)
                .OrderBy(b => b.Title)
                .Select(b => new BookDto(
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Genre,
                    b.Inventory != null ? b.Inventory.Quantity : 0,
                    b.Inventory != null ? b.Inventory.Status : "Không rõ",
                    b.CoverImageUrl
                 ));

            return await dtoQuery.ToPagedResultAsync(pageNumber, pageSize);
        }

        // Lấy danh sách thể loại
        public async Task<PagedResult<GenreDto>> GetGenresAsync(string? search, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var grouped = await _context.Books
                .AsNoTracking()
                .Where(b => !string.IsNullOrEmpty(b.Genre))
                .GroupBy(b => b.Genre!)
                .Select(g => new GenreDto(
                    g.Key,
                    g.Count()
                 ))
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                grouped = grouped
                    .Where(g => g.Name.ToLower().Contains(term))
                    .ToList();
            }

            grouped = grouped.OrderBy(g => g.Name).ToList();

            var totalItems = grouped.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var skip = (pageNumber - 1) * pageSize;

            var items = grouped
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new PagedResult<GenreDto>(items, totalItems, pageNumber, totalPages);
        }

        // Tìm sách theo thể loại
        public async Task<PagedResult<BookDto>> SearchBooksByGenreAsync(string genre, int pageNumber, int pageSize)
        {
            IQueryable<Book> books = _context.Books
                .Where(b => b.Genre == genre);

            var dtoQuery = books
                .Include(b => b.Inventory)
                .OrderBy(b => b.Title)
                .Select(b => new BookDto(
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Genre,
                    b.Inventory != null ? b.Inventory.Quantity : 0,
                    b.Inventory != null ? b.Inventory.Status : "Không rõ",
                    b.CoverImageUrl
                 ));

            return await dtoQuery.ToPagedResultAsync(pageNumber, pageSize);
        }
    }
}
