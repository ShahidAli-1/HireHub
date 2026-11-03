using System.Security.Claims;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Models;
using HireHub.Services.Files;
using HireHub.ViewModels.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Controllers
{
    [Authorize(Roles = RoleConstants.JobSeeker)]
    public class JobSeekerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public JobSeekerController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        // GET: /JobSeeker
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;

            var user = await _context.Users
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            // ==========================================
            // PROFILE COMPLETENESS CALCULATION
            // ==========================================
            var profile = user.JobSeekerProfile;
            int score = 0;

            if (!string.IsNullOrEmpty(user.FullName)) score += 10;
            if (!string.IsNullOrEmpty(user.Phone)) score += 10;
            if (!string.IsNullOrEmpty(profile?.Location)) score += 10;
            if (!string.IsNullOrEmpty(profile?.Bio)) score += 15;
            if (!string.IsNullOrEmpty(profile?.Skills)) score += 15;
            if (!string.IsNullOrEmpty(profile?.Education)) score += 15;
            if (!string.IsNullOrEmpty(profile?.Experience)) score += 15;
            if (!string.IsNullOrEmpty(profile?.ResumePath)) score += 10;

            ViewBag.ProfileScore = score; // Max 100

            // Stats
            var totalApplications = await _context.Applications
                .CountAsync(a => a.JobSeekerId == userId);

            var shortlisted = await _context.Applications
                .CountAsync(a => a.JobSeekerId == userId && a.Status == "Shortlisted");

            var interviews = await _context.Applications
                .CountAsync(a => a.JobSeekerId == userId && a.Status == "Interview");

            var recentApplications = await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Recruiter)
                        .ThenInclude(r => r!.RecruiterProfile)
                .Where(a => a.JobSeekerId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .ToListAsync();

            var recommendedJobs = await _context.Jobs
                .Include(j => j.Recruiter).ThenInclude(r => r!.RecruiterProfile)
                .Include(j => j.Category)
                .Where(j => j.Status == "Active")
                .OrderByDescending(j => j.IsFeatured)
                .ThenByDescending(j => j.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.TotalApplications = totalApplications;
            ViewBag.Shortlisted = shortlisted;
            ViewBag.Interviews = interviews;
            ViewBag.RecentApplications = recentApplications;
            ViewBag.RecommendedJobs = recommendedJobs;
            ViewBag.User = user;

            return View();
        }

        // GET: /JobSeeker/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _context.Users
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            var model = new JobSeekerProfileViewModel
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Bio = user.JobSeekerProfile?.Bio,
                Location = user.JobSeekerProfile?.Location,
                Skills = user.JobSeekerProfile?.Skills,
                Experience = user.JobSeekerProfile?.Experience,
                Education = user.JobSeekerProfile?.Education,
                ExpectedSalary = user.JobSeekerProfile?.ExpectedSalary,
                ExistingResumePath = user.JobSeekerProfile?.ResumePath
            };

            return View(model);
        }

        // POST: /JobSeeker/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(JobSeekerProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Phone = model.Phone;

            if (user.JobSeekerProfile == null)
            {
                user.JobSeekerProfile = new JobSeekerProfile { UserId = CurrentUserId };
            }

            user.JobSeekerProfile.Bio = model.Bio;
            user.JobSeekerProfile.Location = model.Location;
            user.JobSeekerProfile.Skills = model.Skills;
            user.JobSeekerProfile.Experience = model.Experience;
            user.JobSeekerProfile.Education = model.Education;
            user.JobSeekerProfile.ExpectedSalary = model.ExpectedSalary;
            user.JobSeekerProfile.UpdatedAt = DateTime.UtcNow;

            // Handle Resume Upload
            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                try
                {
                    var newPath = await _fileService.SaveFileAsync(model.ResumeFile, "resumes");
                    user.JobSeekerProfile.ResumePath = newPath;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ResumeFile", ex.Message);
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // ==================================================
        // SAVED JOBS
        // ==================================================

        // GET: /JobSeeker/SavedJobs
        [HttpGet]
        public async Task<IActionResult> SavedJobs()
        {
            var savedJobs = await _context.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Category)
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Recruiter)
                        .ThenInclude(r => r!.RecruiterProfile)
                .Where(s => s.UserId == CurrentUserId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            return View(savedJobs);
        }

        // POST: /JobSeeker/ToggleSaveJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSaveJob(int jobId, string? returnUrl)
        {
            var existing = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.UserId == CurrentUserId && s.JobId == jobId);

            if (existing != null)
            {
                _context.SavedJobs.Remove(existing);
                TempData["Success"] = "Job removed from saved list.";
            }
            else
            {
                _context.SavedJobs.Add(new SavedJob
                {
                    UserId = CurrentUserId,
                    JobId = jobId
                });
                TempData["Success"] = "Job saved successfully!";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(SavedJobs));
        }
    }
}