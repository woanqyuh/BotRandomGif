using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotTrungThuong.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadMediaManager([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            try
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov", ".mkv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !permittedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Unsupported file format.");
                }
                string uploadsFolder;
                if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(fileExtension))
                {
                    uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/images");
                }
                else
                {
                    uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/videos");
                }

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName.Replace(" ", "")}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var fileUrl = $"/uploads/{(uploadsFolder.Contains("images") ? "images" : "videos")}/{uniqueFileName}";
                return Ok(new { Success = true, FileUrl = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
