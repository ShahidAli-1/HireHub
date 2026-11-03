using System.Security.Claims;
using HireHub.Helpers; 
using HireHub.Services.Subscriptions;
using HireHub.ViewModels.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireHub.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        private string CurrentRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? RoleConstants.JobSeeker;

        private string CurrentPlan =>
            User.FindFirstValue("CurrentPlan") ?? PlanConstants.Free;

        // GET: /Subscription/Plans
        [HttpGet]
        public IActionResult Plans(bool limitReached = false)
        {
            var plans = CurrentRole == RoleConstants.JobSeeker
                ? PlanCatalog.JobSeekerPlans
                : PlanCatalog.RecruiterPlans;

            var model = new PlansViewModel
            {
                Role = CurrentRole,
                CurrentPlan = CurrentPlan,
                Plans = plans,
                LimitReached = limitReached
            };

            return View(model);
        }

        // GET: /Subscription/MySubscriptions
        [HttpGet]
        public async Task<IActionResult> MySubscriptions()
        {
            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(CurrentUserId);
            return View(subscriptions);
        }

        // POST: /Subscription/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var cancelled = await _subscriptionService.CancelAsync(id, CurrentUserId);
            if (!cancelled)
            {
                TempData["Error"] = "Unable to cancel this subscription.";
            }
            else
            {
                TempData["Success"] = "Subscription cancelled.";
            }
            return RedirectToAction(nameof(MySubscriptions));
        }
    }
}