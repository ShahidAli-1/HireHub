using System.Security.Claims;
using HireHub.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireHub.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("UserId") ?? "0");

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUserId);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteAsync(id, CurrentUserId);
            TempData["Success"] = "Notification deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            await _notificationService.DeleteAllAsync(CurrentUserId);
            TempData["Success"] = "All notifications cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}