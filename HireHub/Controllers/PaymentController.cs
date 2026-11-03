using System.Security.Claims;
using HireHub.Helpers;
using HireHub.Services.Files;
using HireHub.Services.Subscriptions;
using HireHub.ViewModels.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireHub.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IFileService _fileService;

        public PaymentController(ISubscriptionService subscriptionService, IFileService fileService)
        {
            _subscriptionService = subscriptionService;
            _fileService = fileService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        private string CurrentRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? RoleConstants.JobSeeker;

        // GET: /Payment/Submit?plan=Basic
        [HttpGet]
        public IActionResult Submit(string plan)
        {
            if (string.IsNullOrEmpty(plan))
                return RedirectToAction("Plans", "Subscription");

            var planDetails = PlanCatalog.GetPlan(CurrentRole, plan);
            if (planDetails == null || planDetails.Price <= 0)
                return RedirectToAction("Plans", "Subscription");

            var model = new SubmitPaymentViewModel
            {
                PlanName = plan,
                Amount = planDetails.Price
            };

            ViewBag.PlanDetails = planDetails;
            return View(model);
        }

        // POST: /Payment/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitPaymentViewModel model)
        {
            var planDetails = PlanCatalog.GetPlan(CurrentRole, model.PlanName);
            if (planDetails == null || planDetails.Price <= 0)
                return RedirectToAction("Plans", "Subscription");

            if (!ModelState.IsValid)
            {
                ViewBag.PlanDetails = planDetails;
                return View(model);
            }

            // Save proof image if uploaded
            string? proofPath = null;
            if (model.ProofImage != null && model.ProofImage.Length > 0)
            {
                try
                {
                    proofPath = await _fileService.SaveFileAsync(model.ProofImage, "payment-proofs");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProofImage", ex.Message);
                    ViewBag.PlanDetails = planDetails;
                    return View(model);
                }
            }

            var purpose = $"Subscription: {model.PlanName}";
            var payment = await _subscriptionService.SubmitPaymentAsync(
                CurrentUserId,
                planDetails.Price,
                purpose,
                model.ReferenceNumber,
                proofPath,
                model.Notes);

            TempData["Success"] =
                "Payment submitted! Our team will review and activate your plan within 24 hours.";
            return RedirectToAction(nameof(History));
        }

        // GET: /Payment/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var payments = await _subscriptionService.GetUserPaymentsAsync(CurrentUserId);
            return View(payments);
        }
    }
}