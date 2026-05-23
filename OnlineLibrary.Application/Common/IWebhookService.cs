using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Application.Common
{
    public class LoanReminderPayload
    {
        public string BookTitle { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Status { get; set; }
        public decimal EstimatedFine { get; set; }
    }

    public interface IWebhookService
    {
        Task NotifyNewBookAsync(string Title, string Author);
        Task NotifyLoanReminderAsync(IEnumerable<LoanReminderPayload> payloads);
    }
}
