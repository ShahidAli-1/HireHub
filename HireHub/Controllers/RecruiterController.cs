using System.Security.Claims;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.Services.Applications;
using HireHub.Services.Export;
using HireHub.Services.Files;
using HireHub.Services.Jobs;
using HireHub.ViewModels.Applications;
using HireHub.ViewModels.Jobs;
using HireHub.ViewModels.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Controllers
{
    [Authorize(Roles = RoleConstants.Recruiter)]
    public class RecruiterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        private readonly IFileService _fileService;
        private readonly IExportService _exportService;

        public RecruiterController(
            ApplicationDbContext context,
            IJobService jobService,
            IApplicationService applicationService,
            IFileService fileService,
            IExportService exportService)
        {
            _context = context;
            _jobService = jobService;
            _applicationService = applicationService;
            _fileService = fileService;
            _exportService = exportService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        // ==================================================
        // DASHBOARD
        // ==================================================

        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;

            var user = await _context.Users
                .Include(u => u.RecruiterProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            var totalJobs = await _context.Jobs.CountAsync(j => j.RecruiterId == userId);
            var activeJobs = await _context.Jobs.CountAsync(j => j.RecruiterId == userId && j.Status == "Active");

            var totalApplications = await _context.Applications
                .Include(a => a.Job)
                .CountAsync(a => a.Job!.RecruiterId == userId);

            var shortlisted = await _context.Applications
                .Include(a => a.Job)
                .CountAsync(a => a.Job!.RecruiterId == userId && a.Status == "Shortlisted");

            var recentJobs = await _context.Jobs
                .Include(j => j.Category)
                .Where(j => j.RecruiterId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentApplications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
                .Where(a => a.Job!.RecruiterId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalJobs = totalJobs;
            ViewBag.ActiveJobs = activeJobs;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.Shortlisted = shortlisted;
            ViewBag.RecentJobs = recentJobs;
            ViewBag.RecentApplications = recentApplications;
            ViewBag.User = user;

            return View();
        }

        // ==================================================
        // COMPANY PROFILE
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> CompanyProfile()
        {
            var user = await _context.Users
                .Include(u => u.RecruiterProfile)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            var model = new CompanyProfileViewModel
            {
                FullName = user.FullName,
                CompanyName = user.RecruiterProfile?.CompanyName ?? "",
                Industry = user.RecruiterProfile?.Industry,
                CompanySize = user.RecruiterProfile?.CompanySize,
                Website = user.RecruiterProfile?.Website,
                About = user.RecruiterProfile?.About,
                Location = user.RecruiterProfile?.Location,
                ExistingLogoPath = user.RecruiterProfile?.CompanyLogoPath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyProfile(CompanyProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.RecruiterProfile)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            user.FullName = model.FullName;

            if (user.RecruiterProfile == null)
            {
                user.RecruiterProfile = new RecruiterProfile { UserId = CurrentUserId };
            }

            user.RecruiterProfile.CompanyName = model.CompanyName;
            user.RecruiterProfile.Industry = model.Industry;
            user.RecruiterProfile.CompanySize = model.CompanySize;
            user.RecruiterProfile.Website = model.Website;
            user.RecruiterProfile.About = model.About;
            user.RecruiterProfile.Location = model.Location;
            user.RecruiterProfile.UpdatedAt = DateTime.UtcNow;

            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                try
                {
                    var newPath = await _fileService.SaveFileAsync(model.LogoFile, "logos");
                    user.RecruiterProfile.CompanyLogoPath = newPath;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("LogoFile", ex.Message);
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Company profile updated successfully!";
            return RedirectToAction(nameof(CompanyProfile));
        }

        // ==================================================
        // POST JOB
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            var categories = await _jobService.GetActiveCategoriesAsync();

            var model = new PostJobViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _jobService.GetActiveCategoriesAsync();
                model.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList();
                return View(model);
            }

            await _jobService.CreateJobAsync(model, CurrentUserId);
            TempData["Success"] = "Job posted successfully!";
            return RedirectToAction(nameof(MyJobs));
        }

        // ==================================================
        // MY JOBS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> MyJobs()
        {
            var jobs = await _jobService.GetRecruiterJobsAsync(CurrentUserId);
            return View(jobs);
        }

        // ==================================================
        // EDIT JOB
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var job = await _jobService.GetJobForRecruiterAsync(id, CurrentUserId);
            if (job == null) return NotFound();

            var categories = await _jobService.GetActiveCategoriesAsync();

            var model = new EditJobViewModel
            {
                JobId = job.JobId,
                Title = job.Title,
                CategoryId = job.CategoryId,
                Description = job.Description,
                Requirements = job.Requirements,
                SalaryRange = job.SalaryRange,
                Location = job.Location,
                JobType = job.JobType,
                ExperienceLevel = job.ExperienceLevel,
                Deadline = job.Deadline,
                Status = job.Status,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name,
                    Selected = c.CategoryId == job.CategoryId
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(EditJobViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _jobService.GetActiveCategoriesAsync();
                model.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name,
                    Selected = c.CategoryId == model.CategoryId
                }).ToList();
                return View(model);
            }

            var updated = await _jobService.UpdateJobAsync(model, CurrentUserId);
            if (!updated) return NotFound();

            TempData["Success"] = "Job updated successfully!";
            return RedirectToAction(nameof(MyJobs));
        }

        // ==================================================
        // CLOSE JOB
        // ==================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseJob(int id)
        {
            var closed = await _jobService.CloseJobAsync(id, CurrentUserId);
            if (!closed) return NotFound();

            TempData["Success"] = "Job closed successfully.";
            return RedirectToAction(nameof(MyJobs));
        }

        // POST: /Recruiter/DeleteJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var deleted = await _jobService.DeleteJobAsync(id, CurrentUserId);
            if (!deleted)
            {
                TempData["Error"] = "Job not found or you don't have permission.";
            }
            else
            {
                TempData["Success"] = "Job deleted successfully.";
            }

            return RedirectToAction(nameof(MyJobs));
        }

        // POST: /Recruiter/ReopenJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReopenJob(int id)
        {
            var reopened = await _jobService.ReopenJobAsync(id, CurrentUserId);
            if (!reopened)
            {
                TempData["Error"] = "Job could not be reopened.";
            }
            else
            {
                TempData["Success"] = "Job reopened successfully and is now active!";
            }

            return RedirectToAction(nameof(MyJobs));
        }

        // ==================================================
        // APPLICANTS
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Applicants(int? jobId = null, string? status = null)
        {
            var applications = await _applicationService
                .GetApplicationsForRecruiterAsync(CurrentUserId, jobId);

            // Apply Status filter
            if (!string.IsNullOrEmpty(status))
            {
                applications = applications.Where(a => a.Status == status).ToList();
            }

            var jobs = await _jobService.GetRecruiterJobsAsync(CurrentUserId);

            var model = new ApplicantsListViewModel
            {
                Applications = applications,
                RecruiterJobs = jobs,
                SelectedJobId = jobId,
                SelectedStatus = status
            };

            return View(model);
        }

        // GET: /Recruiter/ApplicantDetails/5
        [HttpGet]
        public async Task<IActionResult> ApplicantDetails(int id)
        {
            var app = await _applicationService
                .GetApplicationForRecruiterAsync(id, CurrentUserId);

            if (app == null) return NotFound();

            return View(app);
        }

        // POST: /Recruiter/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status, string? notes, int? redirectJobId)
        {
            var updated = await _applicationService
                .UpdateStatusAsync(applicationId, status, CurrentUserId, notes);

            if (!updated)
            {
                TempData["Error"] = "Unable to update application status.";
            }
            else
            {
                TempData["Success"] = $"Application marked as '{status}'.";
            }

            return RedirectToAction(nameof(ApplicantDetails), new { id = applicationId });
        }

        // POST: /Recruiter/ScheduleInterview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterview([FromBody] InterviewScheduleModel model)
        {
            if (model == null) return BadRequest("Invalid data.");

            string interviewDetails = $"Date: {model.InterviewDate} | Time: {model.InterviewTime} | Link: {model.MeetingLink}";

            var updated = await _applicationService
                .UpdateStatusAsync(model.ApplicationId, "Interview", CurrentUserId, null, interviewDetails);

            if (updated)
                return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Failed to update." });
        }

        // ==================================================
        // EXPORT
        // ==================================================

        // GET: /Recruiter/ExportApplicants
        [HttpGet]
        public async Task<IActionResult> ExportApplicants(int? jobId = null)
        {
            var applications = await _applicationService
                .GetApplicationsForRecruiterAsync(CurrentUserId, jobId);

            var csv = _exportService.ExportApplicationsToCsv(applications);

            string fileName = jobId.HasValue
                ? $"applicants-job-{jobId}-{DateTime.UtcNow:yyyyMMdd}.csv"
                : $"all-applicants-{DateTime.UtcNow:yyyyMMdd}.csv";

            return File(csv, "text/csv", fileName);
        }

        // ==================================================
        // KANBAN BOARD
        // ==================================================

        [HttpGet]
        public async Task<IActionResult> Kanban(int? jobId = null)
        {
            var applications = await _applicationService
                .GetApplicationsForRecruiterAsync(CurrentUserId, jobId);

            var jobs = await _jobService.GetRecruiterJobsAsync(CurrentUserId);

            var model = new ApplicantsListViewModel
            {
                Applications = applications,
                RecruiterJobs = jobs,
                SelectedJobId = jobId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKanbanStatus([FromBody] KanbanUpdateModel model)
        {
            if (model == null) return BadRequest("Invalid data.");

            var updated = await _applicationService
                .UpdateStatusAsync(model.ApplicationId, model.Status, CurrentUserId);

            if (updated)
                return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Failed to update." });
        }
    }

    // ==================================================
    // AJAX MODELS (Keep these at the bottom)
    // ==================================================

    public class KanbanUpdateModel
    {
        public int ApplicationId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}