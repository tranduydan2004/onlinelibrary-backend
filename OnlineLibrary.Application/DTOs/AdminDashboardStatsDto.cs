using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Application.DTOs
{
    public class AdminDashboardStatsDto
    {
        public int TotalBooks { get; set; }

        public int BooksAddedToday { get; set; }
        public int BooksAddedThisWeek { get; set; }
        public int BooksAddedThisMonth { get; set; }

        public int TotalUsers { get; set; }
        public int PendingLoanRequests { get; set; }
    }
}
