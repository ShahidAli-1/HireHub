namespace HireHub.Services.Files
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
        {
            ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"
        };

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("File is too large. Maximum 5 MB.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"Invalid file type. Allowed: {string.Join(", ", AllowedExtensions)}");

            // Generate safe filename
            var uniqueName = $"{Guid.NewGuid()}{ext}";

            // Build path: wwwroot/uploads/{subFolder}/{uniqueName}
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadsRoot);

            var fullPath = Path.Combine(uploadsRoot, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for use in views
            return $"/uploads/{subFolder}/{uniqueName}";
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}