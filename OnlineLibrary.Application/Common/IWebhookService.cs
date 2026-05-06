using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Application.Common
{
    public interface IWebhookService
    {
        Task NotifyNewBookAsync(string Title, string Author);
    }
}
