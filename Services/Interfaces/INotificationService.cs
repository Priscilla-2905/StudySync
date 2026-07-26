namespace StudySync.Services.Interfaces;

/// <summary>
/// Manages local push notifications for reminders and alerts.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Schedules a notification at a specific time.
    /// </summary>
    Task ScheduleNotificationAsync(string title, string message, DateTime notifyTime, int notificationId);

    /// <summary>
    /// Cancels a previously scheduled notification.
    /// </summary>
    Task CancelNotificationAsync(int notificationId);

    /// <summary>
    /// Cancels all scheduled notifications.
    /// </summary>
    Task CancelAllAsync();

    /// <summary>
    /// Schedules all notifications for a user's upcoming events.
    /// </summary>
    Task ScheduleAllRemindersAsync(int userId);
}
