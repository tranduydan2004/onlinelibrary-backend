using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineLibrary.API.Middlewares
{
    public class CheckUserLockedMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserLockedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out var userId))
                {
                    var isLocked = await dbContext.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.IsLocked)
                        .FirstOrDefaultAsync();

                    if (isLocked)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
