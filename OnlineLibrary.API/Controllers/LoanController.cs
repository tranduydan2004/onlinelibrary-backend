using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Services;
using System.Security.Claims;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="User,Admin")] // Cho phép cả User và Admin
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public LoanController(ILoanService loanService) { _loanService = loanService; }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpPost("request")]
        public async Task<IActionResult> RequestLoan(LoanRequestDto dto)
        {
            var result = await _loanService.CreateLoanRequestAsync(GetCurrentUserId(), dto.BookId);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Yêu cầu mượn sách đã được gửi thành công." });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var history = await _loanService.GetLoanHistoryAsync(GetCurrentUserId(), pageNumber, pageSize);
            return Ok(history);
        }

        [HttpPut("extend/{loanId}")]
        public async Task<IActionResult> ExtendLoan(int loanId)
        {
            var result = await _loanService.ExtendLoanAsync(loanId, GetCurrentUserId());
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
            return Ok(new { message = "Gia hạn sách thành công." });
        }
    }
}
