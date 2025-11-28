using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(string username, string password);
        Task<Result<AuthResponseDto>> LoginAsync(string username, string password);
    }
}