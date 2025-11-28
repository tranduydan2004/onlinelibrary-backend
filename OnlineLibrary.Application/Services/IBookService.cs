using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> SearchBooksAsync(string? keyword, int pageNumber, int pageSize);
        Task<Result<BookDto>> GetBookByIdAsync(int id);
    }
}
