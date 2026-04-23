using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Domain.Entities;
using OnlineLibrary.Infrastructure.Data;

namespace OnlineLibrary.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AnyUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> AnyEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string input)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == input || (u.Email != null && u.Email.ToLower() == input.ToLower()));
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User> Items, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Users.AsQueryable();
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<User>> GetLockedUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsLocked)
                .OrderBy(u => u.Username)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }
    }
}
