using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Infrastructure.Data;

namespace OnlineLibrary.Infrastructure.Repositories
{
    public class LoanRequestRepository : ILoanRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public LoanRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanRequest?> GetByIdAsync(int id)
        {
            return await _context.LoanRequests.FindAsync(id);
        }

        public async Task<LoanRequest?> GetByIdWithBookInventoryAsync(int id)
        {
            return await _context.LoanRequests
                .Include(r => r.Book.Inventory)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> AnyActiveLoanByBookIdAsync(int bookId)
        {
            return await _context.LoanRequests
                .AnyAsync(l => l.BookId == bookId && l.Status == "Đang mượn");
        }

        public async Task<bool> AnyActiveLoanByUserAndBookAsync(int userId, int bookId)
        {
            return await _context.LoanRequests
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId && (r.Status == "Đang chờ duyệt" || r.Status == "Đang mượn"));
        }

        public async Task AddLoanRequestAsync(LoanRequest request)
        {
            _context.LoanRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLoanRequestAsync(LoanRequest request)
        {
            _context.LoanRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<LoanRequest> Items, int TotalCount)> GetLoanHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.LoanRequests
                .Include(r => r.Book)
                .Where(r => r.UserId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<LoanRequest> Items, int TotalCount)> GetAllLoansAsync(int pageNumber, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var query = _context.LoanRequests
                .Include(l => l.Book)
                .Include(l => l.User)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.RequestDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(l => l.RequestDate < endExclusive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.RequestDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetPendingLoanRequestsCountAsync()
        {
            return await _context.LoanRequests.CountAsync(l => l.Status == "Đang chờ duyệt");
        }
    }
}
