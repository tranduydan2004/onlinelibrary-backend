using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Application.Extensions;

namespace OnlineLibrary.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;
        public LoanService(ApplicationDbContext context) { _context = context; }

        public async Task<Result> CreateLoanRequestAsync(int userId, int bookId)
        {
            var inventory = await _context.BookInventories.FirstOrDefaultAsync(i => i.BookId == bookId);
            if (inventory == null || inventory.Quantity <= 0)
            {
                return Result.Fail("Sách đã hết hoặc không tồn tại.");
            }

            var existingRequest = await _context.LoanRequests
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId && (r.Status == "Đang chờ duyệt" || r.Status == "Đang mượn"));
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
            _context.LoanRequests.Add(loanRequest);
            inventory.Quantity--;
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> ExtendLoanAsync(int loadId, int userId)
        {
            var loan = await _context.LoanRequests.FirstOrDefaultAsync(l => l.Id == loadId && l.UserId == userId);
            if (loan == null) return Result.Fail("Không tìm thấy yêu cầu mượn.");
            if (loan.Status != "Đang mượn") return Result.Fail("Chỉ có thể gia hạn sách đang muợn.");
            if (loan.DueDate.HasValue && loan.DueDate.Value < DateTimeOffset.UtcNow) return Result.Fail("Sách đã quá hạn, không thể gia hạn.");

            loan.DueDate = (loan.DueDate ?? DateTimeOffset.UtcNow).AddDays(7);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<PagedResult<LoanHistoryDto>> GetLoanHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.LoanRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Book)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new LoanHistoryDto(
                    r.Id,
                    r.Book != null ? r.Book.Title : "[Sách đã bị xóa]",
                    r.RequestDate,
                    r.DueDate,
                    r.Status
                 ))
                .AsNoTracking();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
    }
}
