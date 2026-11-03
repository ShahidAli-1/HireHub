using HireHub.Models;

namespace HireHub.Services.Notifications
{
    public interface INotificationService
    {
        Task CreateAsync(int userId, string title, string message, string type = "General", string? link = null);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);

        // NEW DELETE METHODS
        Task DeleteAsync(int notificationId, int userId);
        Task DeleteAllAsync(int userId);
    }
}