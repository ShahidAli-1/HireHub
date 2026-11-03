using System.Security.Claims;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Services.Applications;
using HireHub.Services.Files;
using HireHub.Services.Jobs;
using HireHub.Services.Usage;
using HireHub.ViewModels.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Controllers
{
    [Authorize(Roles = RoleConstants.JobSeeker)]
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IJobService _jobService;
        private readonly IUsageLimitService _usageService;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;

        public ApplicationsController(
            IApplicationService applicationService,
            IJobService jobService,
            IUsageLimitService usageService,
            IFileService fileService,
            ApplicationDbContext context)
        {
            _applicationService = applicationService;
            _jobService = jobService;
            _usageService = usageService;
            _fileService = fileService;
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        private string CurrentPlan =>
            User.FindFirstValue("CurrentPlan") ?? PlanConstants.Free;

        // ============================================================
        // HELPER: DYNAMIC LAYOUT
        // ============================================================
        private void SetLayoutBasedOnRole()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(RoleConstants.JobSeeker))
                    ViewData["Layout"] = "_JobSeekerLayout";
                else if (User.IsInRole(RoleConstants.Recruiter))
                    ViewData["Layout"] = "_RecruiterLayout";
                else if (User.IsInRole(RoleConstants.Admin))
                    ViewData["Layout"] = "_AdminLayout";
            }
            else
            {
                ViewData["Layout"] = "_Layout";
            }
        }

        // ============================================================
        // APPLY
        // ============================================================

        // GET: /Applications/Apply/5
        [HttpGet]
        public async Task<IActionResult> Apply(int jobId)
        {
            SetLayoutBasedOnRole(); // <-- Add this line to keep the sidebar

            var job = await _jobService.GetPublicJobDetailAsync(jobId);
            if (job == null) return NotFound();

            if (await _applicationService.HasAppliedAsync(jobId, CurrentUserId))
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction("Detail", "Jobs", new { id = jobId });
            }

            var model = await BuildApplyViewModelAsync(jobId);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /Applications/Apply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyJobViewModel input)
        {
            SetLayoutBasedOnRole(); // <-- Add this line to keep the sidebar on validation error

            var model = await BuildApplyViewModelAsync(input.JobId);
            if (model == null) return NotFound();

            model.CoverLetter = input.CoverLetter;
            model.UseNewResume = input.UseNewResume;
            model.NewResumeFile = input.NewResumeFile;

            if (model.UseNewResume && (input.NewResumeFile == null || input.NewResumeFile.Length == 0))
            {
                ModelState.AddModelError("NewResumeFile", "Please select a file to upload.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var limit = PlanLimits.GetJobSeekerLimit(CurrentPlan);
            var allowed = await _usageService.CheckAndIncrementAsync(CurrentUserId, "Application", limit);

            if (!allowed)
            {
                TempData["Error"] = $"You've reached your daily limit of {limit} applications. Upgrade your plan to continue.";
                return RedirectToAction("Plans", "Subscription");
            }

            try
            {
                string? customResumePath = null;

                if (model.UseNewResume && input.NewResumeFile != null)
                {
                    customResumePath = await _fileService.SaveFileAsync(input.NewResumeFile, "resumes");
                }

                await _applicationService.ApplyAsync(
                    input.JobId, CurrentUserId, input.CoverLetter, customResumePath);

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction(nameof(MyApplications));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ============================================================
        // MY APPLICATIONS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            SetLayoutBasedOnRole(); // <-- Ensure this page also keeps the sidebar

            var apps = await _applicationService.GetSeekerApplicationsAsync(CurrentUserId);

            var (appsToday, _) = await _usageService.GetTodayUsageAsync(CurrentUserId);
            var limit = PlanLimits.GetJobSeekerLimit(CurrentPlan);

            ViewBag.ApplicationsToday = appsToday;
            ViewBag.DailyLimit = limit;
            ViewBag.CurrentPlan = CurrentPlan;

            return View(apps);
        }

        // POST: /Applications/Withdraw/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var success = await _applicationService
                .WithdrawApplicationAsync(id, CurrentUserId);

            if (!success)
            {
                TempData["Error"] = "Unable to withdraw this application.";
            }
            else
            {
                TempData["Success"] = "Application withdrawn. You can now apply again.";
            }

            return RedirectToAction(nameof(MyApplications));
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            SetLayoutBasedOnRole(); // <-- Ensure this page also keeps the sidebar

            var app = await _applicationService.GetByIdAsync(id, CurrentUserId);
            if (app == null) return NotFound();

            return View(app);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private async Task<ApplyJobViewModel?> BuildApplyViewModelAsync(int jobId)
        {
            var job = await _jobService.GetPublicJobDetailAsync(jobId);
            if (job == null) return null;

            var user = await _context.Users
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            return new ApplyJobViewModel
            {
                JobId = jobId,
                Job = job,
                ProfileResumePath = user?.JobSeekerProfile?.ResumePath,
                HasProfileResume = !string.IsNullOrEmpty(user?.JobSeekerProfile?.ResumePath)
            };
        }
    }
}