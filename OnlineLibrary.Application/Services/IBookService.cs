using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> SearchBooksAsync(string? keyword, int pageNumber, int pageSize);
        Task<Result<BookDto>> GetBookByIdAsync(int id);

        // Danh sách thể loại
        Task<PagedResult<GenreDto>> GetGenresAsync(string? search, int pageNumber, int pageSize);

        // Tìm sách theo thể loại
        Task<PagedResult<BookDto>> SearchBooksByGenreAsync(string genre, int pageNumber, int pageSize);
    }
}
