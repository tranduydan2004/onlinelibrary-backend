using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineLibrary.API.HostedServices
{
    public class DailyLoanReminderJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyLoanReminderJob> _logger;

        public DailyLoanReminderJob(IServiceProvider serviceProvider, ILogger<DailyLoanReminderJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayToNext8AM();
                _logger.LogInformation($"Next loan reminder job scheduled in {delay.TotalHours} hours.");
                
                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ProcessRemindersAsync(stoppingToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Loan reminder job was cancelled.");
                }
            }
        }

        private TimeSpan GetDelayToNext8AM()
        {
            var now = DateTimeOffset.UtcNow;
            var vietnamOffset = TimeSpan.FromHours(7); // Múi giờ Việt Nam (UTC+7)
            var vietnamTime = now.ToOffset(vietnamOffset);

            var next8AM = new DateTimeOffset(vietnamTime.Year, vietnamTime.Month, vietnamTime.Day, 8, 0, 0, vietnamOffset);
            
            if (vietnamTime >= next8AM)
            {
                next8AM = next8AM.AddDays(1);
            }

            return next8AM - vietnamTime;
        }

        private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Daily Loan Reminder Job...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var loanRequestRepo = scope.ServiceProvider.GetRequiredService<ILoanRequestRepository>();
                var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

                var activeLoans = await loanRequestRepo.GetActiveLoansForReminderAsync();
                var payloads = new List<LoanReminderPayload>();

                var now = DateTimeOffset.UtcNow;
                var vietnamOffset = TimeSpan.FromHours(7);
                var today = now.ToOffset(vietnamOffset).Date;

                foreach (var loan in activeLoans)
                {
                    if (!loan.DueDate.HasValue) continue;

                    var dueDate = loan.DueDate.Value.ToOffset(vietnamOffset).Date;
                    var daysDiff = (dueDate - today).Days;

                    string originalStatus = loan.Status;
                    string status = loan.Status;
                    decimal fineAmount = 0;

                    if (daysDiff == 1) // Ngày mai là hạn chót
                    {
                        status = LoanRequestStatus.DueSoon;
                    }
                    else if (daysDiff < 0) // Quá hạn
                    {
                        status = LoanRequestStatus.Overdue;
                        int overdueDays = -daysDiff;
                        fineAmount = overdueDays * 5000;
                    }

                    if (status != originalStatus)
                    {
                        loan.Status = status;
                        await loanRequestRepo.UpdateLoanRequestAsync(loan);
                    }

                    // Prepare payload for those DueSoon or Overdue
                    if (status == LoanRequestStatus.DueSoon || status == LoanRequestStatus.Overdue)
                    {
                        payloads.Add(new LoanReminderPayload
                        {
                            BookTitle = loan.Book?.Title ?? "N/A",
                            Email = loan.User?.Email,
                            PhoneNumber = loan.User?.PhoneNumber,
                            Status = status,
                            EstimatedFine = fineAmount
                        });
                    }
                }

                if (payloads.Count > 0)
                {
                    await webhookService.NotifyLoanReminderAsync(payloads);
                    _logger.LogInformation($"Sent {payloads.Count} reminders to webhook.");
                }

                _logger.LogInformation("Daily Loan Reminder Job completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Daily Loan Reminder Job: {ex.Message}");
            }
        }
    }
}
