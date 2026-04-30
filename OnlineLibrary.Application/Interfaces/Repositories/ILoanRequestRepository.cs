using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Application.Interfaces.Repositories
{
    public interface ILoanRequestRepository
    {
        Task<LoanRequest?> GetByIdAsync(int id);
        Task<LoanRequest?> GetByIdWithBookInventoryAsync(int id);
        Task<bool> AnyActiveLoanByBookIdAsync(int bookId);
        Task<bool> AnyActiveLoanByUserAndBookAsync(int userId, int bookId);
        Task AddLoanRequestAsync(LoanRequest request);
        Task UpdateLoanRequestAsync(LoanRequest request);
        Task<(List<LoanRequest> Items, int TotalCount)> GetLoanHistoryAsync(int userId, int pageNumber, int pageSize);
        Task<(List<LoanRequest> Items, int TotalCount)> GetAllLoansAsync(int pageNumber, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate);
        Task<int> GetPendingLoanRequestsCountAsync();
    }
}
