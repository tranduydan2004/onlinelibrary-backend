namespace OnlineLibrary.Application.DTOs
{
    public record RegisterDto(string Username, string Password, string Email, string PhoneNumber);
    public record LoginDto(string Username, string Password);
    public record AuthResponseDto(string Token, string Username, string Role);

    public record VerifyEmailDto(string Username, string OtpCode);
    public record UserProfileDto(int Id, string Username, string Role, string? Email, string? PhoneNumber, bool EmailConfirmed);
    public record UpdateProfileDto(string Email, string PhoneNumber);
    public record ForgotPasswordDto(string UsernameOrEmail);
    public record ResetPasswordDto(string Username, string OtpCode, string NewPassword);
}
