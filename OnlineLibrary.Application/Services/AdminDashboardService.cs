using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardStatsDto> GetStatsAsync()
        {
            var today = DateTimeOffset.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);
            var monthStart = new DateTimeOffset(new DateTime(today.Year, today.Month, 1), TimeSpan.Zero);

            var totalBooks = await _context.Books.CountAsync();

            var booksAddedToday = await _context.Books
                .CountAsync(b => b.CreatedAt.Date == today);

            var booksAddedThisWeek = await _context.Books
                .CountAsync(b => b.CreatedAt.Date >= sevenDaysAgo);

            var booksAddedThisMonth = await _context.Books
                .CountAsync(b => b.CreatedAt.Date >= monthStart);

            var totalUsers = await _context.Users.CountAsync();

            var pendingLoanRequests = await _context.LoanRequests
                .CountAsync(l => l.Status == "Đang chờ duyệt");

            return new AdminDashboardStatsDto
            {
                TotalBooks = totalBooks,
                BooksAddedToday = booksAddedToday,
                BooksAddedThisWeek = booksAddedThisWeek,
                BooksAddedThisMonth = booksAddedThisMonth,
                TotalUsers = totalUsers,
                PendingLoanRequests = pendingLoanRequests
            };
        }
    }
}
