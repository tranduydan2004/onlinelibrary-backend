using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Infrastructure.Data;
using System.Runtime.InteropServices;
using OnlineLibrary.Application.Extensions;

namespace OnlineLibrary.Application.Services
{
    public class BookAdminService : IBookAdminService
    {
        private readonly ApplicationDbContext _context;
        public BookAdminService(ApplicationDbContext context) { _context = context; }

        public async Task<Result<BookDto>> AddBookAsync(BookCreateDto dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Genre = dto.Genre,
                CoverImageUrl = dto.CoverImageUrl,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var inventory = new BookInventory
            {
                Book = book,
                Quantity = dto.InitialQuantity,
                Status = dto.InitialQuantity > 0 ? "Có sẵn" : "Hết sách"
            };

            _context.Books.Add(book);
            _context.BookInventories.Add(inventory);

            await _context.SaveChangesAsync();

            // Map sang DTO để trả ra ngoài, tránh vòng tham chiếu
            var bookDto = new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Genre,
                inventory.Quantity,
                inventory.Status,
                book.CoverImageUrl
            );

            return Result<BookDto>.Ok(bookDto);
        }

        public async Task<Result> UpdateBookAsync(int id, BookUpdateDto dto)
        {
            // 1. Lấy Book + Inventory
            var book = await _context.Books
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return Result.Fail("Không tìm thấy sách.");

            // 2. Cập nhật thông tin sách
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Genre = dto.Genre;
            book.CoverImageUrl = dto.CoverImageUrl;

            // 3. Cập nhật thông tin kho
            if (book.Inventory == null)
            {
                // Tạo mới bản ghi kho nếu chưa có
                book.Inventory = new BookInventory
                {
                    BookId = book.Id,
                    Quantity = dto.Quantity,
                    Status = dto.Quantity > 0 ? "Có sẵn" : "Hết sách"
                };
            }
            else
            {
                // Cập nhật số lượng và trạng thái kho
                book.Inventory.Quantity = dto.Quantity;
                book.Inventory.Status = dto.Quantity > 0 ? "Có sẵn" : "Hết sách";
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return Result.Fail("Không tìm thấy sách.");

            // Kiểm tra nghiệp vụ: Không cho xóa sách nếu đang có người mượn
            if (await _context.LoanRequests.AnyAsync(l => l.BookId == id && l.Status == "Đang mượn"))
            {
                return Result.Fail("Không thể xóa sách đang có người mượn.");
            }

            // Xóa bản ghi kho liên quan
            if (book.Inventory != null)
            {
                _context.BookInventories.Remove(book.Inventory);
            }
            // Xóa sách
            _context.Books.Remove(book);

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<PagedResult<BookDto>> GetAllBooksAsync(int pageNumber, int pageSize)
        {
            IQueryable<BookDto> query = _context.Books
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
                 ))
                .AsNoTracking();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
    }
}
