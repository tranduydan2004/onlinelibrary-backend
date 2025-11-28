using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public interface ILoanService
    {
        Task<Result> CreateLoanRequestAsync(int userId, int bookId);
        Task<Result> ExtendLoanAsync(int loadId, int userId);
        Task<PagedResult<LoanHistoryDto>> GetLoanHistoryAsync(int userId, int pageNumber, int pageSize);
    }
}
