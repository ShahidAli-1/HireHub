using HireHub.Helpers;
using HireHub.Services.ATS;
using HireHub.Services.Usage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HireHub.Controllers
{
    public class ResumeToolsController : Controller
    {
        private readonly IUsageLimitService _usageService;
        private readonly IAtsService _atsService;

        public ResumeToolsController(IUsageLimitService usageService, IAtsService atsService)
        {
            _usageService = usageService;
            _atsService = atsService;
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

        // ==================================================
        // ATS SCORING
        // ==================================================

        // GET: /ResumeTools
        [HttpGet]
        public IActionResult Index()
        {
            SetLayoutBasedOnRole();
            TempData.Remove("Error");
            return View();
        }

        // POST: /ResumeTools/Analyze
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(IFormFile ResumeFile)
        {
            SetLayoutBasedOnRole();

            if (ResumeFile == null)
            {
                TempData["Error"] = "Please upload a resume.";
                return View("Index");
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                var plan = User.FindFirstValue("CurrentPlan") ?? PlanConstants.Free;

                int dailyLimit = PlanLimits.GetResumeScoreLimit(plan);

                if (dailyLimit != int.MaxValue)
                {
                    var allowed = await _usageService.CheckAndIncrementAsync(userId, "ResumeScore", dailyLimit);

                    if (!allowed)
                    {
                        TempData["Error"] = $"You've reached your daily limit of {dailyLimit} resume scores on the {plan} plan. Upgrade for unlimited.";
                        return View("Index");
                    }
                }
            }
            else
            {
                var guestUsed = HttpContext.Session.GetString("GuestResumeUsed");
                if (guestUsed == "true")
                {
                    TempData["Error"] = "You have used your 1 free public preview. Please create a free account to continue checking resumes.";
                    return View("Index");
                }
            }

            try
            {
                var resumeText = await _atsService.ExtractTextAsync(ResumeFile);

                if (string.IsNullOrWhiteSpace(resumeText))
                {
                    TempData["Error"] = "Could not read text from the file. Is it an image-based PDF?";
                    return View("Index");
                }

                var (score, passed, failed) = _atsService.CalculateGeneralScore(resumeText);

                if (User.Identity?.IsAuthenticated != true)
                {
                    HttpContext.Session.SetString("GuestResumeUsed", "true");
                }

                TempData["AtsScore"] = score;
                TempData["Passed"] = string.Join("||", passed);
                TempData["Failed"] = string.Join("||", failed);

                return RedirectToAction(nameof(Result));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error analyzing file: {ex.Message}";
                return View("Index");
            }
        }

        // GET: /ResumeTools/Result
        [HttpGet]
        public IActionResult Result()
        {
            SetLayoutBasedOnRole();

            if (TempData["AtsScore"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Score = Convert.ToInt32(TempData["AtsScore"]);
            ViewBag.Passed = TempData["Passed"]?.ToString().Split("||", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            ViewBag.Failed = TempData["Failed"]?.ToString().Split("||", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            return View();
        }
    }
}