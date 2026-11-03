namespace HireHub.Services.ATS
{
    public interface IAtsService
    {
        Task<string> ExtractTextAsync(IFormFile file);
        (int Score, List<string> PassedChecks, List<string> FailedChecks) CalculateGeneralScore(string resumeText);
    }
}