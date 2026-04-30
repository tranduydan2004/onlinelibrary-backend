using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.DTOs;
using OnlineLibrary.Application.Interfaces.Repositories;

namespace OnlineLibrary.Application.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserRepository _userRepository;

        public UserAdminService(IUserRepository userRepository) 
        { 
            _userRepository = userRepository; 
        }

        public async Task<Result<string>> ToggleUserLockAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return Result<string>.Fail("Không tìm thấy người dùng.");
            if (user.Role == "Admin") return Result<string>.Fail("Không thể khóa tài khoản Admin.");

            // Logic "toggle" (lật/chuyển đổi)
            user.IsLocked = !user.IsLocked; // Nếu tài khoản chưa khóa, logic này sẽ gán giá trị mới sẽ trở thành tài khoản bị khóa và ngược lại
            await _userRepository.UpdateUserAsync(user);

            string message = user.IsLocked ? "Đã khóa tài khoản thành công." : "Đã mở khóa tài khoản thành công.";
            return Result<string>.Ok(message);
        }

        public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var result = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);

            var items = result.Items.Select(u => new AdminUserDto(
                u.Id,
                u.Username,
                u.Role,
                u.IsLocked,
                u.Email,
                u.PhoneNumber
            )).ToList();

            return new PagedResult<AdminUserDto>(items, pageNumber, pageSize, result.TotalCount);
        }

        public async Task<List<AdminUserDto>> GetLockedUsersAsync()
        {
            var users = await _userRepository.GetLockedUsersAsync();

            return users.Select(u => new AdminUserDto(
                u.Id,
                u.Username,
                u.Role,
                u.IsLocked,
                u.Email,
                u.PhoneNumber
            )).ToList();
        }
    }
}
