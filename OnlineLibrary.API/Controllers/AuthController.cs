using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Services;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto.Username, dto.Password);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto.Username, dto.Password);
            if ( !result.IsSuccess)
            {
                return Unauthorized(new { error = result.Error });
            }
            return Ok(result.Value);
        }
    }
}
