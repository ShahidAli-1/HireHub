using System.Security.Claims;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Services.Jobs;
using HireHub.ViewModels.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Controllers
{
    public class JobsController : Controller
    {
        private readonly IJobService _jobService;
        private readonly ApplicationDbContext _context;

        public JobsController(IJobService jobService, ApplicationDbContext context)
        {
            _jobService = jobService;
            _context = context;
        }

        // ==================================================
        // HELPER: DYNAMIC LAYOUT
        // ==================================================
        private void SetLayoutBasedOnRole()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(RoleConstants.JobSeeker))
                {
                    ViewData["Layout"] = "_JobSeekerLayout";
                }
                else if (User.IsInRole(RoleConstants.Recruiter))
                {
                    ViewData["Layout"] = "_RecruiterLayout";
                }
                else if (User.IsInRole(RoleConstants.Admin))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
            }
            else
            {
                ViewData["Layout"] = "_Layout";
            }
        }

        // GET: /Jobs
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? jobType,
            string? location,
            int page = 1)
        {
            SetLayoutBasedOnRole();

            var filter = new JobListViewModel
            {
                Search = search,
                CategoryId = categoryId,
                JobType = jobType,
                Location = location,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = 10
            };

            var result = await _jobService.GetPublicJobsAsync(filter);

            // Fetch applied job IDs if logged in as Job Seeker
            if (User.Identity?.IsAuthenticated == true && User.IsInRole(RoleConstants.JobSeeker))
            {
                var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

                var appliedIds = await _context.Applications
                    .Where(a => a.JobSeekerId == userId)
                    .Select(a => a.JobId)
                    .ToListAsync();

                result.AppliedJobIds = new HashSet<int>(appliedIds);
            }

            return View(result);
        }

        // GET: /Jobs/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            SetLayoutBasedOnRole();

            var job = await _jobService.GetPublicJobDetailAsync(id);
            if (job == null) return NotFound();

            // Get the real-time applicant count
            var applicantCount = await _context.Applications.CountAsync(a => a.JobId == id);
            ViewBag.ApplicantCount = applicantCount;

            bool alreadyApplied = false;
            bool isSaved = false;

            if (User.Identity?.IsAuthenticated == true && User.IsInRole(RoleConstants.JobSeeker))
            {
                var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

                alreadyApplied = await _context.Applications
                    .AnyAsync(a => a.JobId == id && a.JobSeekerId == userId);

                isSaved = await _context.SavedJobs
                    .AnyAsync(s => s.JobId == id && s.UserId == userId);
            }

            ViewBag.AlreadyApplied = alreadyApplied;
            ViewBag.IsSaved = isSaved;

            return View(job);
        }
    }
}