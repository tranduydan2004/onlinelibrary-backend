using OnlineLibrary.Application.DTOs;

namespace OnlineLibrary.Application.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(int userId);
        Task<ProfileUpdateResult> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);
    }

    public class ProfileUpdateResult
    {
        public bool NotFound { get; set; }
        public bool EmailChanged { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
