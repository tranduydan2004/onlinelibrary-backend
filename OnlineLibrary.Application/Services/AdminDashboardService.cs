using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Interfaces.Repositories;

namespace OnlineLibrary.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRequestRepository _loanRequestRepository;

        public AdminDashboardService(
            IBookRepository bookRepository, 
            IUserRepository userRepository, 
            ILoanRequestRepository loanRequestRepository)
        {
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<AdminDashboardStatsDto> GetStatsAsync()
        {
            var today = DateTimeOffset.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);
            var monthStart = new DateTimeOffset(new DateTime(today.Year, today.Month, 1), TimeSpan.Zero);

            var totalBooks = await _bookRepository.GetTotalBooksCountAsync();
            var booksAddedToday = await _bookRepository.GetBooksAddedOnDateAsync(today);
            var booksAddedThisWeek = await _bookRepository.GetBooksAddedSinceAsync(sevenDaysAgo);
            var booksAddedThisMonth = await _bookRepository.GetBooksAddedSinceAsync(monthStart);

            var totalUsers = await _userRepository.GetTotalUsersCountAsync();

            var pendingLoanRequests = await _loanRequestRepository.GetPendingLoanRequestsCountAsync();

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
