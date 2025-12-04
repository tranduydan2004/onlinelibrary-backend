using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Infrastructure.Data;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Extensions;

namespace OnlineLibrary.Application.Services
{
    public class LoanAdminService : ILoanAdminService
    {
        private readonly ApplicationDbContext _context;
        public LoanAdminService(ApplicationDbContext context) { _context = context; }

        public async Task<Result> ApproveLoanRequestAsync(int requestId)
        {
            var request = await _context.LoanRequests.FindAsync(requestId);
            if (request == null || request.Status != "Đang chờ duyệt")
                return Result.Fail("Yêu cầu không hợp lệ hoặc đã được xử lý.");

            request.Status = "Đang mượn";
            request.DueDate = DateTimeOffset.UtcNow.AddDays(7); // Mượn trong 7 ngày
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> RejectLoanRequestAsync(int requestId)
        {
            var request = await _context.LoanRequests
                .Include(r => r.Book.Inventory) // Phải Include Inventory để cộng lại số lượng sách
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Status != "Đang chờ duyệt")
            {
                return Result.Fail("Yêu cầu không hợp lệ hoặc đã được xử lý.");
            }

            // Đánh dấu từ chối
            request.Status = "Bị từ chối";

            // Hoàn trả lại số lượng sách vào kho
            // (Vì khi user request mượn, ta đã trừ tạm 1 cuốn)
            request.Book.Inventory.Quantity++;

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> ConfirmReturnAsync(int requestId)
        {
            var request = await _context.LoanRequests.Include(r => r.Book.Inventory)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            
            if (request == null || (request.Status != "Đang mượn" && request.Status != "Quá hạn"))
            {
                return Result.Fail("Yêu cầu không hợp lệ (không ở trạng thái 'Đang mượn' hoặc 'Quá hạn'.");
            }

            request.Status = "Đã trả";

            // Cập nhật lại kho sách
            request.Book.Inventory.Quantity++;

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<PagedResult<AdminLoanDto>> GetAllLoansAsync(int pageNumber, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var query = _context.LoanRequests
                .Include(l => l.Book)
                .Include(l => l.User)
                .AsQueryable();

            // Filter theo ngày
            if (fromDate.HasValue)
            {
                query = query.Where(l => l.RequestDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Lấy đến hết ngày toDate ( < ngày + 1 )
                var endExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(l => l.RequestDate < endExclusive);
            }
            
            var projectedQuery = query
                .OrderByDescending(l => l.RequestDate)
                .Select(l => new AdminLoanDto(
                    l.Id,
                    l.Book.Title,
                    l.User.Username,
                    l.RequestDate,
                    l.DueDate,
                    l.Status
                 ))
                .AsNoTracking();

            return await projectedQuery.ToPagedResultAsync(pageNumber, pageSize);
        }
    }
}
