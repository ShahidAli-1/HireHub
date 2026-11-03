using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.ViewModels.Account;
using HireHub.ViewModels.Auth;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        public AuthService(ApplicationDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // ================= REGISTER & LOGIN =================

        public async Task<(bool Success, string Error, User? User)> RegisterAsync(RegisterViewModel model)
        {
            if (model.Role != RoleConstants.JobSeeker && model.Role != RoleConstants.Recruiter)
                return (false, "Invalid role selected", null);

            var exists = await _context.Users.AnyAsync(u => u.Email == model.Email.Trim().ToLower());
            if (exists)
                return (false, "Email is already registered", null);

            var user = new User
            {
                Email = model.Email.Trim().ToLower(),
                FullName = model.FullName.Trim(),
                Phone = model.Phone?.Trim(),
                Role = model.Role,
                PasswordHash = _passwordService.HashPassword(model.Password),
                IsActive = true,
                IsVerified = false,
                CurrentPlan = "Free",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create empty profiles based on role
            if (user.Role == RoleConstants.JobSeeker)
            {
                _context.JobSeekerProfiles.Add(new JobSeekerProfile { UserId = user.UserId });
            }
            else if (user.Role == RoleConstants.Recruiter)
            {
                _context.RecruiterProfiles.Add(new RecruiterProfile
                {
                    UserId = user.UserId,
                    CompanyName = "Not Set"
                });
            }
            await _context.SaveChangesAsync();

            return (true, "", user);
        }

        public async Task<(bool Success, string Error, User? User)> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email.Trim().ToLower());

            if (user == null)
                return (false, "Invalid email or password", null);

            if (!user.IsActive)
                return (false, "Your account has been deactivated. Contact admin.", null);

            if (!_passwordService.VerifyPassword(model.Password, user.PasswordHash))
                return (false, "Invalid email or password", null);

            return (true, "", user);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ================= ACCOUNT SETTINGS =================

        public async Task<(bool Success, string Error)> UpdateAccountAsync(int userId, AccountSettingsViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "User not found.");

            // Check if email is already taken by someone else
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email.Trim().ToLower() && u.UserId != userId);
            if (emailExists)
                return (false, "This email is already registered to another account.");

            // Update basic info
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim().ToLower();
            user.Phone = model.Phone?.Trim();

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // Verify current password
                if (string.IsNullOrEmpty(model.CurrentPassword) ||
                    !_passwordService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    return (false, "Current password is incorrect.");
                }

                // Hash and save new password
                user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();
            return (true, "");
        }

        // ================= FORGOT & RESET PASSWORD =================

        public async Task<(bool Success, string Token)> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null) return (false, "");

            // Generate a secure random token
            var token = Guid.NewGuid().ToString("N");

            user.PasswordResetToken = token;
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15); // Token valid for 15 mins

            await _context.SaveChangesAsync();
            return (true, token);
        }

        public async Task<(bool Success, string Error)> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email.Trim().ToLower());
            if (user == null) return (false, "Invalid request.");

            if (user.PasswordResetToken != model.Token || user.ResetTokenExpiresAt < DateTime.UtcNow)
                return (false, "The reset link is invalid or has expired. Please request a new one.");

            // Update password and clear token
            user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();
            return (true, "");
        }
    }
}