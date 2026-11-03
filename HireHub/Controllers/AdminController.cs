using System.Security.Claims;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Services.Auth;
using HireHub.Services.Export;
using HireHub.Services.Subscriptions;
using HireHub.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAuthService _authService;
        private readonly IPasswordService _passwordService;
        private readonly IExportService _exportService;

        public AdminController(
            ApplicationDbContext context,
            ISubscriptionService subscriptionService,
            IAuthService authService,
            IPasswordService passwordService,
            IExportService exportService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _authService = authService;
            _passwordService = passwordService;
            _exportService = exportService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        // ==================================================
        // DASHBOARD
        // ==================================================

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalRecruiters = await _context.Users.CountAsync(u => u.Role == RoleConstants.Recruiter);
            var totalJobs = await _context.Jobs.CountAsync();
            var activeJobs = await _context.Jobs.CountAsync(j => j.Status == "Active");
            var totalApplications = await _context.Applications.CountAsync();

            var approvedPayments = await _context.Payments
                .Where(p => p.Status == "Approved")
                .ToListAsync();
            var totalRevenue = approvedPayments.Sum(p => p.Amount);

            var activeSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status == "Active");

            var pendingPayments = await _context.Payments
                .CountAsync(p => p.Status == "Pending");

            var pendingRecruiters = await _context.RecruiterProfiles
                .CountAsync(r => !r.IsVerified);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentPayments = await _context.Payments
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRecruiters = totalRecruiters;
            ViewBag.TotalJobs = totalJobs;
            ViewBag.ActiveJobs = activeJobs;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.ActiveSubscriptions = activeSubscriptions;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.PendingRecruiters = pendingRecruiters;
            ViewBag.RecentUsers = recentUsers;
            ViewBag.RecentPayments = recentPayments;

            return View();
        }

        // ==================================================
        // USERS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Users(string? search, string? role, bool? active)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(s)
                                      || u.Email.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            if (active.HasValue)
                query = query.Where(u => u.IsActive == active.Value);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var model = new UsersListViewModel
            {
                Users = users,
                Search = search,
                RoleFilter = role,
                ActiveFilter = active
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.UserId == CurrentUserId)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Users));
        }

        // ==================================================
        // RECRUITERS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Recruiters()
        {
            var recruiters = await _context.Users
                .Include(u => u.RecruiterProfile)
                .Where(u => u.Role == RoleConstants.Recruiter)
                .OrderBy(u => u.RecruiterProfile!.IsVerified)
                .ThenByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(new RecruitersListViewModel { Recruiters = recruiters });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRecruiter(int id)
        {
            var profile = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profile == null) return NotFound();

            profile.IsVerified = true;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = CurrentUserId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Recruiter verified.";
            return RedirectToAction(nameof(Recruiters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnverifyRecruiter(int id)
        {
            var profile = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profile == null) return NotFound();

            profile.IsVerified = false;
            profile.VerifiedAt = null;
            profile.VerifiedBy = null;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Verification removed.";
            return RedirectToAction(nameof(Recruiters));
        }

        // ==================================================
        // CATEGORIES
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(new CategoriesListViewModel { Categories = categories });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid category details.";
                return RedirectToAction(nameof(Categories));
            }

            var category = new Models.Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Icon = model.Icon?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryFormModel model)
        {
            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null) return NotFound();

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();
            category.Icon = model.Icon?.Trim();
            category.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryActive(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Category {(category.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Categories));
        }

        // ==================================================
        // JOBS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Jobs(string? status, string? search)
        {
            var query = _context.Jobs
                .Include(j => j.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .Include(j => j.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(j => j.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(s));
            }

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.Search = search;
            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.Status = "Suspended";
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Job suspended.";
            return RedirectToAction(nameof(Jobs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReinstateJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.Status = "Active";
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Job reinstated.";
            return RedirectToAction(nameof(Jobs));
        }

        // ==================================================
        // PAYMENTS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            var pending = await _subscriptionService.GetPendingPaymentsAsync();

            var model = new PendingPaymentsViewModel
            {
                PendingPayments = pending,
                TotalPending = pending.Sum(p => p.Amount)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            var (success, error) = await _subscriptionService
                .ApprovePaymentAsync(id, CurrentUserId);

            if (success)
                TempData["Success"] = "Payment approved and subscription activated.";
            else
                TempData["Error"] = error;

            return RedirectToAction(nameof(Payments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Please provide a rejection reason.";
                return RedirectToAction(nameof(Payments));
            }

            var (success, error) = await _subscriptionService
                .RejectPaymentAsync(id, CurrentUserId, reason);

            if (success)
                TempData["Success"] = "Payment rejected.";
            else
                TempData["Error"] = error;

            return RedirectToAction(nameof(Payments));
        }

        // ==================================================
        // REVENUE (NEW)
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Revenue()
        {
            var allApproved = await _context.Payments
                .Include(p => p.User)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == "Active")
                .ToListAsync();

            // ===== Compute revenue by month (last 6 months) =====
            var months = new List<string>();
            var monthlyRevenue = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                months.Add(targetDate.ToString("MMM yyyy"));

                var monthTotal = allApproved
                    .Where(p => p.CreatedAt.Year == targetDate.Year
                             && p.CreatedAt.Month == targetDate.Month)
                    .Sum(p => p.Amount);

                monthlyRevenue.Add(monthTotal);
            }

            // ===== Plan distribution =====
            var planDistribution = activeSubscriptions
                .GroupBy(s => s.PlanType)
                .ToDictionary(g => g.Key, g => g.Count());

            var revenueByPlan = allApproved
                .Where(p => p.Purpose.StartsWith("Subscription:"))
                .GroupBy(p => p.Purpose.Replace("Subscription:", "").Trim())
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            // ===== MRR: sum of this month's approved subscription payments =====
            var currentMonth = DateTime.UtcNow;
            var mrr = allApproved
                .Where(p => p.CreatedAt.Year == currentMonth.Year
                         && p.CreatedAt.Month == currentMonth.Month
                         && p.Purpose.StartsWith("Subscription:"))
                .Sum(p => p.Amount);

            var model = new RevenueReportViewModel
            {
                TotalRevenue = allApproved.Sum(p => p.Amount),
                MRR = mrr,
                TotalTransactions = allApproved.Count,
                ActiveSubscriptions = activeSubscriptions.Count,
                MonthLabels = months,
                MonthlyRevenue = monthlyRevenue,
                PlanDistribution = planDistribution,
                RevenueByPlan = revenueByPlan,
                RecentTransactions = allApproved.Take(10).ToList()
            };

            return View(model);
        }

        // GET: /Admin/ExportRevenue
        [HttpGet]
        public async Task<IActionResult> ExportRevenue()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var csv = _exportService.ExportPaymentsToCsv(payments);
            return File(csv, "text/csv", $"hirehub-revenue-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ==================================================
        // CREATE ADMIN
        // ==================================================

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new CreateAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var exists = await _context.Users
                .AnyAsync(u => u.Email == model.Email.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var admin = new Models.User
            {
                Email = model.Email.Trim().ToLower(),
                FullName = model.FullName.Trim(),
                Phone = model.Phone?.Trim(),
                Role = RoleConstants.Admin,
                PasswordHash = _passwordService.HashPassword(model.Password),
                IsActive = true,
                IsVerified = true,
                CurrentPlan = "Free",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Admin account created for {admin.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}