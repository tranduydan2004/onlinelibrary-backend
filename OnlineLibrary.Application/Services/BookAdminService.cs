using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Application.Interfaces.Repositories;

namespace OnlineLibrary.Application.Services
{
    public class BookAdminService : IBookAdminService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IWebhookService _webhookService;

        public BookAdminService(IBookRepository bookRepository, ILoanRequestRepository loanRequestRepository, IWebhookService webhookService) 
        { 
            _bookRepository = bookRepository; 
            _loanRequestRepository = loanRequestRepository;
            _webhookService = webhookService; 
        }

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

            await _bookRepository.AddBookAsync(book, inventory);

            // Nếu lưu thành công, gọi webhook bắn thông báo
            // Việc này chạy ngầm, không ảnh hưởng đến CSDL
            _ = _webhookService.NotifyNewBookAsync(dto.Title, dto.Author);

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
            var book = await _bookRepository.GetByIdWithInventoryAsync(id);

            if (book == null) return Result.Fail("Không tìm thấy sách.");

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Genre = dto.Genre;
            book.CoverImageUrl = dto.CoverImageUrl;

            if (book.Inventory == null)
            {
                book.Inventory = new BookInventory
                {
                    BookId = book.Id,
                    Quantity = dto.Quantity,
                    Status = dto.Quantity > 0 ? "Có sẵn" : "Hết sách"
                };
            }
            else
            {
                book.Inventory.Quantity = dto.Quantity;
                book.Inventory.Status = dto.Quantity > 0 ? "Có sẵn" : "Hết sách";
            }

            await _bookRepository.UpdateBookAsync(book);
            return Result.Ok();
        }

        public async Task<Result> DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdWithInventoryAsync(id);
            if (book == null) return Result.Fail("Không tìm thấy sách.");

            if (await _loanRequestRepository.AnyActiveLoanByBookIdAsync(id))
            {
                return Result.Fail("Không thể xóa sách đang có người mượn.");
            }

            await _bookRepository.DeleteBookAsync(book);
            return Result.Ok();
        }

        public async Task<PagedResult<BookDto>> GetAllBooksAsync(int pageNumber, int pageSize)
        {
            var result = await _bookRepository.GetAllBooksAsync(pageNumber, pageSize);

            var items = result.Items.Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Inventory != null ? b.Inventory.Quantity : 0,
                b.Inventory != null ? b.Inventory.Status : "Không rõ",
                b.CoverImageUrl
            )).ToList();

            return new PagedResult<BookDto>(items, pageNumber, pageSize, result.TotalCount);
        }

        public async Task<List<BookDto>> GetOutOfStockBooksAsync()
        {
            var books = await _bookRepository.GetOutOfStockBooksAsync();

            return books.Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Inventory != null ? b.Inventory.Quantity : 0,
                b.Inventory != null ? b.Inventory.Status : "Không rõ",
                b.CoverImageUrl
            )).ToList();
        }

        public async Task<List<BookDto>> GetTopQuantityBooksAsync(int topN)
        {
            var books = await _bookRepository.GetTopQuantityBooksAsync(topN);

            return books.Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Inventory != null ? b.Inventory.Quantity : 0,
                b.Inventory != null ? b.Inventory.Status : "Không rõ",
                b.CoverImageUrl
            )).ToList();
        }
    }
}
