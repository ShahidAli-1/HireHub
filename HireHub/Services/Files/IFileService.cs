namespace HireHub.Services.Files
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
        void DeleteFile(string relativePath);
    }
}