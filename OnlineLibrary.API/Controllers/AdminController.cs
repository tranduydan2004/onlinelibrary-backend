using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Services;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/admin")] // Định tuyến cho AdminController, sử dụng tiền tố "admin" cho tất cả API
    [Authorize(Roles = "Admin")] // Yêu cầu role Admin mới truy cập được
    public class AdminController : ControllerBase
    {
        private readonly IBookAdminService _bookAdminService;
        private readonly IUserAdminService _userAdminService;
        private readonly ILoanAdminService _loanAdminService;
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(IBookAdminService book, IUserAdminService user, ILoanAdminService loan, IAdminDashboardService dashboardService)
        {
            _bookAdminService = book;
            _userAdminService = user;
            _loanAdminService = loan;
            _dashboardService = dashboardService;
        }

        // --- QUẢN LÝ SÁCH (CRUD) ---
        [HttpPost("book")]
        public async Task<IActionResult> AddBook(BookCreateDto dto)
        {
            var result = await _bookAdminService.AddBookAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            // Trả về 201 Created cùng với thông tin sách vừa tạo
            return CreatedAtAction(nameof(AddBook), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto dto)
        {
            var result = await _bookAdminService.UpdateBookAsync(id, dto);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return NoContent(); // Trả về 204 No Content khi cập nhật thành công
        }

        [HttpDelete("book/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookAdminService.DeleteBookAsync(id);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return NoContent(); // Trả về 204 No Content khi xóa thành công
        }

        [HttpGet("book/all")]
        public async Task<IActionResult> GetAllBooks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var books = await _bookAdminService.GetAllBooksAsync(pageNumber, pageSize);
            return Ok(books);
        }

        // --- QUẢN LÝ NGƯỜI DÙNG ---
        [HttpPut("user/{id}/toggle-lock")]
        public async Task<IActionResult> ToggleLockUser(int id)
        {
            var result = await _userAdminService.ToggleUserLockAsync(id);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = result.Value }); // Trả về thông báo khóa/mở khóa thành công
        }

        [HttpGet("user/all")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userAdminService.GetAllUsersAsync(pageNumber, pageSize);
            return Ok(users);
        }

        // --- QUẢN LÝ YÊU CẦU MƯỢN/TRẢ ---
        [HttpPut("loan/{requestId}/approve")]
        public async Task<IActionResult> ApproveLoan(int requestId)
        {
            var result = await _loanAdminService.ApproveLoanRequestAsync(requestId);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Phê duyệt yêu cầu thành công." });
        }

        [HttpPut("loan/{requestId}/reject")]
        public async Task<IActionResult> RejectLoan(int requestId)
        {
            var result = await _loanAdminService.RejectLoanRequestAsync(requestId);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Từ chối yêu cầu thành công." });
        }

        [HttpPut("loan/{requestId}/return")]
        public async Task<IActionResult> ConfirmReturn(int requestId)
        {
            var result = await _loanAdminService.ConfirmReturnAsync(requestId);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Xác nhận trả sách thành công." });
        }

        [HttpGet("loan/all")]
        public async Task<IActionResult> GetAllLoans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var loans = await _loanAdminService.GetAllLoansAsync(pageNumber, pageSize, fromDate, toDate);
            return Ok(loans);
        }

        // --- THỐNG KÊ DASHBOARD ---
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboard()
        {
            var stats = await _dashboardService.GetStatsAsync();
            return Ok(stats);
        }
    }
}
