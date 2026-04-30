using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> AnyUsernameAsync(string username);
        Task<bool> AnyEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string input);
        Task<User?> GetByIdAsync(int id);
        Task UpdateUserAsync(User user);
        Task<(List<User> Items, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<List<User>> GetLockedUsersAsync();
        Task<int> GetTotalUsersCountAsync();
    }
}
