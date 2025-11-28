namespace OnlineLibrary.Application.DTOs
{
    public record LoanRequestDto(int BookId);
    public record LoanHistoryDto(int Id, string BookTitle, DateTimeOffset RequestDate, DateTimeOffset? DueDate, string Status);
}
