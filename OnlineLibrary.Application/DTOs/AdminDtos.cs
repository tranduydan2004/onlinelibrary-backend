namespace OnlineLibrary.Application.DTOs
{
    // Admin xem danh sách người dùng
    public record AdminUserDto(int Id, string Username, string Role, bool IsLocked);
    // Admin xem danh sách yêu cầu mượn/trả sách
    public record AdminLoanDto(int Id, string BookTitle, string Username, DateTimeOffset RequestDate, DateTimeOffset? DueDate, string Status);
}
