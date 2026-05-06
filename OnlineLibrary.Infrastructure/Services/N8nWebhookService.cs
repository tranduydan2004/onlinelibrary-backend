using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Infrastructure.Services
{
    public class N8nWebhookService : IWebhookService
    {
        private readonly HttpClient _httpclient;
        private readonly IConfiguration _configuration;

        public N8nWebhookService(HttpClient httpclient, IConfiguration configuration)
        {
            _httpclient = httpclient;
            _configuration = configuration;
        }

        public async Task NotifyNewBookAsync(string Title, string Author)
        {
            string webhookUrl = _configuration["ExternalServices:N8nWebhookUrl"];

            // Kiểm tra an toàn nếu lỡ quên cấu hình
            if (string.IsNullOrEmpty(webhookUrl)) {
                Console.WriteLine("CẢNH BÁO: Chưa cấu hình Webhook URL cho n8n!");
                return;
            }

            // Payload dữ liệu
            var bookData = new
            {
                tieu_de = Title,
                tac_gia = Author,
                thoi_gian_them = DateTimeOffset.Now
            };

            // Chuyển đổi Object C# sang chuỗi JSON
            var jsonPayload = JsonSerializer.Serialize(bookData);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try {
                // Gửi request POST sang n8n
                await _httpclient.PostAsync(webhookUrl, content);
            }
            catch (Exception ex) {
                // Ghi log nếu có lỗi (tránh làm sập chức năng thêm sách)
                Console.WriteLine($"Lỗi khi gọi Webhook n8n: {ex.Message}");
            }
        }
    }
}
