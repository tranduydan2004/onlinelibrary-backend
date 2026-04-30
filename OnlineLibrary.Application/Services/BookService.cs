using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Application.Interfaces.Repositories;

namespace OnlineLibrary.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Result<BookDto>> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdWithInventoryAsync(id);

            if (book == null)
            {
                return Result<BookDto>.Fail("Không tìm thấy sách.");
            }

            var bookDto = new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Genre,
                book.Inventory != null ? book.Inventory.Quantity : 0,
                book.Inventory != null ? book.Inventory.Status : "Không rõ",
                book.CoverImageUrl
            );

            return Result<BookDto>.Ok(bookDto);
        }

        public async Task<PagedResult<BookDto>> SearchBooksAsync(string? keyword, int pageNumber, int pageSize)
        {
            var result = await _bookRepository.SearchBooksAsync(keyword, pageNumber, pageSize);

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

        public async Task<PagedResult<GenreDto>> GetGenresAsync(string? search, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _bookRepository.GetGenresAsync(search, pageNumber, pageSize);
            
            var items = result.Items.Select(g => new GenreDto(g.Name, g.Count)).ToList();
            
            return new PagedResult<GenreDto>(items, pageNumber, pageSize, result.TotalCount);
        }

        public async Task<PagedResult<BookDto>> SearchBooksByGenreAsync(string genre, int pageNumber, int pageSize)
        {
            var result = await _bookRepository.SearchBooksByGenreAsync(genre, pageNumber, pageSize);

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
    }
}
