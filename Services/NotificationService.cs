using Plugin.LocalNotification;
using StudySync.Data;
using INotificationService = StudySync.Services.Interfaces.INotificationService;

namespace StudySync.Services;

/// <summary>
/// Service for scheduling local notifications for lectures, assignments, exams, and study sessions.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly DatabaseService _db;

    public NotificationService(DatabaseService db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task ScheduleNotificationAsync(string title, string message, DateTime notifyTime, int notificationId)
    {
        if (notifyTime <= DateTime.Now)
            return;

        try
        {
            var request = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = title,
                Description = message,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }
        catch
        {
            // Fail gracefully if notification permissions are denied or platform is unsupported
        }
    }

    /// <inheritdoc/>
    public Task CancelNotificationAsync(int notificationId)
    {
        try
        {
            LocalNotificationCenter.Current.Cancel(notificationId);
        }
        catch
        {
            // Ignore failure
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task CancelAllAsync()
    {
        try
        {
            LocalNotificationCenter.Current.CancelAll();
        }
        catch
        {
            // Ignore failure
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task ScheduleAllRemindersAsync(int userId)
    {
        var prefs = await _db.GetPreferencesAsync(userId);
        if (!prefs.NotificationsEnabled)
        {
            await CancelAllAsync();
            return;
        }

        // Schedule upcoming assignment reminders (1 day before)
        var assignments = await _db.GetPendingAssignmentsAsync(userId);
        foreach (var assignment in assignments)
        {
            var notifyTime = assignment.Deadline.AddDays(-1);
            if (notifyTime > DateTime.Now)
            {
                await ScheduleNotificationAsync(
                    "Assignment Due Tomorrow!",
                    $"'{assignment.Title}' is due on {assignment.Deadline:MMM dd, hh:mm tt}",
                    notifyTime,
                    10000 + assignment.Id);
            }
        }

        // Schedule upcoming exam reminders (3 days before and 1 day before)
        var exams = await _db.GetUpcomingExamsAsync(userId);
        foreach (var exam in exams)
        {
            var course = await _db.GetByIdAsync<Models.Course>(exam.CourseId);
            var courseName = course?.CourseCode ?? "Exam";

            var notifyTime3Days = exam.ExamDate.AddDays(-3);
            if (notifyTime3Days > DateTime.Now)
            {
                await ScheduleNotificationAsync(
                    "Exam Coming Up!",
                    $"{courseName} exam is in 3 days on {exam.ExamDate:MMM dd} at {exam.Venue}",
                    notifyTime3Days,
                    20000 + exam.Id);
            }

            var notifyTime1Day = exam.ExamDate.AddDays(-1);
            if (notifyTime1Day > DateTime.Now)
            {
                await ScheduleNotificationAsync(
                    "Exam Tomorrow!",
                    $"{courseName} exam tomorrow at {exam.Venue} ({exam.Time:hh\\:mm})",
                    notifyTime1Day,
                    30000 + exam.Id);
            }
        }

        // Schedule today's study session notifications
        var todaySessions = await _db.GetStudySessionsAsync(userId, DateTime.Now);
        foreach (var session in todaySessions)
        {
            if (session.Completed || session.IsBreak)
                continue;

            var sessionStartDateTime = session.Date.Date + session.StartTime;
            if (sessionStartDateTime > DateTime.Now)
            {
                await ScheduleNotificationAsync(
                    "Study Session Time!",
                    $"Time to study for '{session.Title}'",
                    sessionStartDateTime,
                    40000 + session.Id);
            }
        }
    }
}
