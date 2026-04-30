using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Application.Interfaces.Repositories;

namespace OnlineLibrary.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IBookRepository _bookRepository;

        public LoanService(ILoanRequestRepository loanRequestRepository, IBookRepository bookRepository) 
        { 
            _loanRequestRepository = loanRequestRepository; 
            _bookRepository = bookRepository;
        }

        public async Task<Result> CreateLoanRequestAsync(int userId, int bookId)
        {
            var book = await _bookRepository.GetByIdWithInventoryAsync(bookId);
            if (book == null || book.Inventory == null || book.Inventory.Quantity <= 0)
            {
                return Result.Fail("Sách đã hết hoặc không tồn tại.");
            }

            var existingRequest = await _loanRequestRepository.AnyActiveLoanByUserAndBookAsync(userId, bookId);
            if (existingRequest)
            {
                return Result.Fail("Bạn đã yêu cầu mượn hoặc đang mượn cuốn sách này.");
            }

            var loanRequest = new LoanRequest
            {
                UserId = userId,
                BookId = bookId,
                RequestDate = DateTimeOffset.UtcNow,
                Status = "Đang chờ duyệt"
            };
            
            book.Inventory.Quantity--;
            
            await _loanRequestRepository.AddLoanRequestAsync(loanRequest);
            // Việc cập nhật sách sẽ được lưu vào DB, có thể sử dụng UpdateBookAsync nếu cần thiết, 
            // tuy nhiên AddLoanRequestAsync đã gọi SaveChangesAsync() trên DbContext chung (scoped) 
            // nên Quantity-- cũng đã được lưu. Dù sao gọi thêm UpdateBookAsync để tường minh:
            await _bookRepository.UpdateBookAsync(book);
            
            return Result.Ok();
        }

        public async Task<Result> ExtendLoanAsync(int loadId, int userId)
        {
            const int MaxExtensionCount = 1; // Số lần gia hạn tối đa cho mỗi lần mượn

            var loan = await _loanRequestRepository.GetByIdAsync(loadId);

            if (loan == null || loan.UserId != userId) return Result.Fail("Không tìm thấy yêu cầu mượn.");

            if (loan.Status != "Đang mượn") return Result.Fail("Chỉ có thể gia hạn sách đang muợn.");

            if (loan.DueDate.HasValue && loan.DueDate.Value < DateTimeOffset.UtcNow) return Result.Fail("Sách đã quá hạn, không thể gia hạn.");

            if (loan.ExtensionCount >= MaxExtensionCount) return Result.Fail($"Bạn đã gia hạn tối đa {MaxExtensionCount} lần cho cuốn sách này.");

            // Gia hạn thêm 7 ngày kể từ ngày hết hạn
            loan.DueDate = (loan.DueDate ?? DateTimeOffset.UtcNow).AddDays(7);
            loan.ExtensionCount++;

            await _loanRequestRepository.UpdateLoanRequestAsync(loan);
            return Result.Ok();
        }

        public async Task<PagedResult<LoanHistoryDto>> GetLoanHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            const int MaxExtensionCount = 1;
            var now = DateTimeOffset.UtcNow;

            var result = await _loanRequestRepository.GetLoanHistoryAsync(userId, pageNumber, pageSize);

            var items = result.Items.Select(r => new LoanHistoryDto(
                r.Id,
                r.Book != null ? r.Book.Title : "[Sách đã bị xóa]",
                r.RequestDate,
                r.DueDate,
                r.Status,
                r.Status == "Đang mượn"
                    && r.DueDate.HasValue
                    && r.DueDate.Value >= now
                    && r.ExtensionCount < MaxExtensionCount
            )).ToList();

            return new PagedResult<LoanHistoryDto>(items, pageNumber, pageSize, result.TotalCount);
        }
    }
}
