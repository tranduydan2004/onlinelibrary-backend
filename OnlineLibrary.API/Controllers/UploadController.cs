using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController (IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("book-cover")]
        [RequestSizeLimit(10_000_000)] // Tối đa ~10MB
        public async Task<IActionResult> UploadBookCover(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File không hợp lệ." });
            }

            // Chỉ cho phép ảnh
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                return BadRequest(new { error = "Chỉ chấp nhận ảnh JPG, PNG, WEBP." });
            }

            // Xác định thư mục wwwroot/uploads/covers
            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadFolder = Path.Combine(webRootPath, "uploads", "covers");
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(file.FileName); // .jpg, .png, ...
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về URL đầy đủ, FE dùng luôn
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/uploads/covers/{fileName}";

            return Ok(new { url });
        }
    }

}
