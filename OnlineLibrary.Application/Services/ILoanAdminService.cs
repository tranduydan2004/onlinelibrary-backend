using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;

namespace OnlineLibrary.Application.Services
{
    public interface ILoanAdminService
    {
        Task<Result> ApproveLoanRequestAsync(int requestId);
        Task<Result> RejectLoanRequestAsync(int requestId);
        Task<Result> ConfirmReturnAsync(int requestId);
        Task<PagedResult<AdminLoanDto>> GetAllLoansAsync(int pageNumber, int pageSize);
    }
}
