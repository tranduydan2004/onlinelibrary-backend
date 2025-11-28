using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Application.Services
{
    public interface IBookAdminService
    {
        Task<Result<BookDto>> AddBookAsync(BookCreateDto dto);
        Task<Result> UpdateBookAsync(int id, BookUpdateDto dto);
        Task<Result> DeleteBookAsync(int id);
        Task<PagedResult<BookDto>> GetAllBooksAsync(int pageNumber, int pageSize);
    }
}
