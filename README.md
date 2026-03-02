# Online Library Backend

Một hệ thống backend mạnh mẽ và linh hoạt dành cho nền tảng Quản lý Thư viện Trực tuyến. Dự án được thiết kế dựa trên các tiêu chuẩn phát triển phần mềm hiện đại, ưu tiên tính dễ bảo trì, dễ mở rộng và hiệu năng cao.

## Giới thiệu

Đây là hệ thống API (Application Programming Interface) cung cấp toàn bộ các nghiệp vụ lõi cho một thư viện trực tuyến. Dự án được áp dụng **Clean Architecture**, giúp phân tách rõ ràng các tầng logic, từ giao diện API đến tương tác với cơ sở dữ liệu.

## Công nghệ sử dụng

- **Framework:** .NET (C#)
- **Kiến trúc:** Clean Architecture
- **Containerization:** Docker & Docker Compose
- **Database:** PostgreSQL / SQL Server (Cấu hình qua Entity Framework Core)

## Cấu trúc dự án

Dự án tuân thủ nghiêm ngặt mô hình Clean Architecture với 4 phân hệ (layers) chính:

- `OnlineLibrary.Domain/`: Chứa các Entities, Enums và Core logic của hệ thống. Tầng này độc lập và không phụ thuộc vào bất kỳ tầng nào khác.
- `OnlineLibrary.Application/`: Chứa Business Logic (Use cases, Interfaces, DTOs). Đóng vai trò cầu nối xử lý các yêu cầu từ API và gọi đến Domain.
- `OnlineLibrary.Infrastructure/`: Xử lý các tác vụ giao tiếp với bên ngoài như Database (Cấu hình EF Core), Repositories, File system hoặc External APIs.
- `OnlineLibrary.API/`: Tầng giao diện (RESTful API), đóng vai trò tiếp nhận HTTP requests/responses, Controllers và cấu hình Middleware.

## Hướng dẫn cài đặt & Chạy dự án

Bạn có thể khởi chạy dự án nhanh chóng thông qua Docker hoặc bằng .NET CLI.

### Khởi chạy với Docker Compose (Khuyên dùng)
Dự án đã tích hợp sẵn `docker-compose.yml` để dễ dàng đóng gói ứng dụng và thiết lập các dịch vụ đi kèm.

1. Clone dự án về máy:
   ```bash
   git clone [https://github.com/tranduydan2004/onlinelibrary-backend.git](https://github.com/tranduydan2004/onlinelibrary-backend.git)
   cd onlinelibrary-backend
2. Thiết lập các biến môi trường cần thiết tại file .env (nếu cần thay đổi).
3. Build và khởi chạy các containers:
   ```bash
   docker-compose up -d --build

### Khởi chạy với .NET CLI

1. Trỏ terminal vào thư mục API:
   ```bash
   cd OnlineLibrary.API
2. Cập nhật chuỗi kết nối Database (Connection String) trong file appsettings.json.
3. Chạy lệnh:
   ```bash
   dotnet restore
   dotnet run

#### Tính năng cốt lõi
- Quản lý Sách: Thêm, sửa, xóa, tìm kiếm thông tin sách.
- Quản lý Người dùng: Xác thực và phân quyền (Admin, Reader).
- Quản lý Mượn/Trả: Xử lý logic mượn sách, theo dõi hạn trả và gia hạn.
Dự án sẽ có thể được mở rộng, tích hợp các tính năng mới trong tương lai gần.
