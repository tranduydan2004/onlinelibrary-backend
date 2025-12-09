using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;

namespace OnlineLibrary.Application.Services
{
    public interface IUserAdminService
    {
        Task<Result<string>> ToggleUserLockAsync(int userId);
        Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<List<AdminUserDto>> GetLockedUsersAsync();
    }
}
