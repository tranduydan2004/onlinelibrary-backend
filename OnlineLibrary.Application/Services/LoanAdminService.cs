using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Application.Services
{
    public class LoanAdminService : ILoanAdminService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IBookRepository _bookRepository;
        
        public LoanAdminService(ILoanRequestRepository loanRequestRepository, IBookRepository bookRepository) 
        { 
            _loanRequestRepository = loanRequestRepository; 
            _bookRepository = bookRepository;
        }

        public async Task<Result> ApproveLoanRequestAsync(int requestId)
        {
            var request = await _loanRequestRepository.GetByIdAsync(requestId);
            if (request == null || request.Status != LoanRequestStatus.Pending)
                return Result.Fail("Yêu cầu không hợp lệ hoặc đã được xử lý.");

            request.Status = LoanRequestStatus.Borrowing;
            request.DueDate = DateTimeOffset.UtcNow.AddDays(7); // Mượn trong 7 ngày
            await _loanRequestRepository.UpdateLoanRequestAsync(request);
            return Result.Ok();
        }

        public async Task<Result> RejectLoanRequestAsync(int requestId)
        {
            var request = await _loanRequestRepository.GetByIdWithBookInventoryAsync(requestId);

            if (request == null || request.Status != LoanRequestStatus.Pending)
            {
                return Result.Fail("Yêu cầu không hợp lệ hoặc đã được xử lý.");
            }

            // Đánh dấu từ chối
            request.Status = LoanRequestStatus.Rejected;

            // Hoàn trả lại số lượng sách vào kho
            // (Vì khi user request mượn, ta đã trừ tạm 1 cuốn)
            if (request.Book != null && request.Book.Inventory != null)
            {
                request.Book.Inventory.Quantity++;
                await _bookRepository.UpdateBookAsync(request.Book);
            }

            await _loanRequestRepository.UpdateLoanRequestAsync(request);
            return Result.Ok();
        }

        public async Task<Result> ConfirmReturnAsync(int requestId)
        {
            var request = await _loanRequestRepository.GetByIdWithBookInventoryAsync(requestId);
            
            if (request == null || (request.Status != LoanRequestStatus.Borrowing && request.Status != LoanRequestStatus.Overdue && request.Status != LoanRequestStatus.DueSoon))
            {
                return Result.Fail("Yêu cầu không hợp lệ (không ở trạng thái 'Đang mượn', 'Sắp đến hạn' hoặc 'Quá hạn'.");
            }

            request.Status = LoanRequestStatus.Returned;
            request.ReturnDate = DateTimeOffset.UtcNow;

            if (request.DueDate.HasValue && request.ReturnDate.Value.Date > request.DueDate.Value.Date)
            {
                int overdueDays = (request.ReturnDate.Value.Date - request.DueDate.Value.Date).Days;
                request.FineAmount = overdueDays * 5000;
            }

            // Cập nhật lại kho sách
            if (request.Book != null && request.Book.Inventory != null)
            {
                request.Book.Inventory.Quantity++;
                await _bookRepository.UpdateBookAsync(request.Book);
            }

            await _loanRequestRepository.UpdateLoanRequestAsync(request);
            return Result.Ok();
        }

        public async Task<PagedResult<AdminLoanDto>> GetAllLoansAsync(int pageNumber, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var result = await _loanRequestRepository.GetAllLoansAsync(pageNumber, pageSize, fromDate, toDate);

            var items = result.Items.Select(l => new AdminLoanDto(
                l.Id,
                l.Book?.Title ?? "[Sách đã xóa]",
                l.User?.Username ?? "[User đã xóa]",
                l.RequestDate,
                l.DueDate,
                l.Status
            )).ToList();

            return new PagedResult<AdminLoanDto>(items, pageNumber, pageSize, result.TotalCount);
        }
    }
}
