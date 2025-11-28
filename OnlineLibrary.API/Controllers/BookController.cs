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
    }
}
