using HireHub.Models;
using HireHub.ViewModels.Auth;

namespace HireHub.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Error, User? User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Error, User? User)> LoginAsync(LoginViewModel model);
        Task<(bool Success, string Error)> UpdateAccountAsync(int userId, HireHub.ViewModels.Account.AccountSettingsViewModel model);
        Task<(bool Success, string Token)> GeneratePasswordResetTokenAsync(string email);
        Task<(bool Success, string Error)> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateLastLoginAsync(int userId);
    }
}