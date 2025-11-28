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
    }
}
