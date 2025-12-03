using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.Services;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập để xem/tìm sách
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService) 
        {
            _bookService = bookService;
        }

        // Tìm kiếm sách có phân trang
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string? keyword = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var books = await _bookService.SearchBooksAsync(keyword, pageNumber, pageSize);
            return Ok(books);
        }

        // Lấy thông tin chi tiết sách theo ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var result = await _bookService.GetBookByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { error = result.Error });
            }
            return Ok(result.Value);
        }

        // Lấy danh sách thể loại
        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres([FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var genres = await _bookService.GetGenresAsync(search, pageNumber, pageSize);
            return Ok(genres);
        }

        // Lấy sách theo thể loại
        [HttpGet("by-genre")]
        public async Task<IActionResult> GetBooksByGenre([FromQuery] string genre, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(genre))
            {
                return BadRequest(new { error = "Thể loại không được để trống." });
            }

            var books = await _bookService.SearchBooksByGenreAsync(genre, pageNumber, pageSize);
            return Ok(books);
        }
    }
}
