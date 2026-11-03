using System.Security.Claims;
using HireHub.Helpers;
using HireHub.Services.Auth;
using HireHub.Services.Email;
using HireHub.ViewModels.Account;
using HireHub.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HireHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AccountController(
            IAuthService authService,
            IEmailService emailService,
            IConfiguration config)
        {
            _authService = authService;
            _emailService = emailService;
            _config = config;
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectBasedOnRole();

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var (success, error, user) = await _authService.LoginAsync(model);

            if (!success || user == null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);

            // NEW: Clear guest session limit upon login so it doesn't block logged-in users
            HttpContext.Session.Remove("GuestResumeUsed");

            await _authService.UpdateLastLoginAsync(user.UserId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectBasedOnRole(user.Role);
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register(string? role = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectBasedOnRole();

            var model = new RegisterViewModel();
            if (role == RoleConstants.Recruiter)
                model.Role = RoleConstants.Recruiter;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, error, user) = await _authService.RegisterAsync(model);

            if (!success || user == null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            await SignInUserAsync(user, rememberMe: false);

            // NEW: Clear guest session limit upon registration
            HttpContext.Session.Remove("GuestResumeUsed");

            await _authService.UpdateLastLoginAsync(user.UserId);

            TempData["Success"] = "Welcome to HireHub! Complete your profile to get started.";
            return RedirectBasedOnRole(user.Role);
        }

        // ================= LOGOUT =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ================= ACCOUNT SETTINGS =================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null) return NotFound();

            var model = new AccountSettingsViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            };

            if (User.IsInRole(RoleConstants.Admin))
                ViewData["Layout"] = "_AdminLayout";
            else if (User.IsInRole(RoleConstants.Recruiter))
                ViewData["Layout"] = "_RecruiterLayout";
            else
                ViewData["Layout"] = "_JobSeekerLayout";

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var (success, error) = await _authService.UpdateAccountAsync(userId, model);

            if (!success)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "Account updated successfully! Please log in with your new details.";
            return RedirectToAction("Login");
        }

        // ================= FORGOT PASSWORD =================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, token) = await _authService.GeneratePasswordResetTokenAsync(model.Email);

            if (success && !string.IsNullOrEmpty(token))
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var resetLink = $"{baseUrl}/Account/ResetPassword?token={token}&email={model.Email}";

                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4f46e5;'>Reset Your Password</h2>
                        <p>Hi,</p>
                        <p>We received a request to reset the password for your HireHub account.</p>
                        <p>Click the button below to choose a new password. This link is valid for <strong>15 minutes</strong>.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>Reset Password</a>
                        </div>
                        <p>If you didn't request this, you can safely ignore this email.</p>
                    </div>";

                await _emailService.SendEmailAsync(model.Email, "Reset Your Password - HireHub", emailBody);
            }

            ViewBag.SuccessMessage = "If an account exists with that email, a password reset link has been sent.";
            return View();
        }

        // ================= RESET PASSWORD =================

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _authService.ResetPasswordAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "Your password has been reset. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // ================= HELPERS =================

        private async Task SignInUserAsync(HireHub.Models.User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new("UserId", user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role),
                new("CurrentPlan", user.CurrentPlan)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(14) : DateTime.UtcNow.AddHours(2)
                });
        }

        private IActionResult RedirectBasedOnRole(string? role = null)
        {
            role ??= User.FindFirst(ClaimTypes.Role)?.Value;

            return role switch
            {
                RoleConstants.Admin => RedirectToAction("Index", "Admin"),
                RoleConstants.Recruiter => RedirectToAction("Index", "Recruiter"),
                RoleConstants.JobSeeker => RedirectToAction("Index", "JobSeeker"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}