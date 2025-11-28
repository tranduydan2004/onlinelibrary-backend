namespace OnlineLibrary.Application.DTOs
{
    public record BookCreateDto(string Title, string Author, string Genre, int InitialQuantity, string? CoverImageUrl);
    public record BookUpdateDto(string Title, string Author, string Genre,int Quantity, string? CoverImageUrl);
    public record BookDto(int Id, string Title, string Author, string Genre, int Quantity, string Status, string? CoverImageUrl);
}
