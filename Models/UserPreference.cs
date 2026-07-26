using SQLite;

namespace StudySync.Models;

/// <summary>
/// Stores user study preferences used by the schedule generator.
/// </summary>
[Table("Preferences")]
public class UserPreference
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public int UserId { get; set; }

    /// <summary>
    /// Preferred study start time stored as ticks (e.g., 6 PM).
    /// </summary>
    public long PreferredStartTicks { get; set; } = TimeSpan.FromHours(8).Ticks;

    /// <summary>
    /// Preferred study end time stored as ticks (e.g., 10 PM).
    /// </summary>
    public long PreferredEndTicks { get; set; } = TimeSpan.FromHours(22).Ticks;

    /// <summary>
    /// Maximum daily study hours.
    /// </summary>
    public double DailyLimit { get; set; } = 4.0;

    /// <summary>
    /// Preferred study session duration in minutes.
    /// </summary>
    public int SessionLength { get; set; } = 60;

    /// <summary>
    /// Break duration between sessions in minutes.
    /// </summary>
    public int BreakLength { get; set; } = 15;

    /// <summary>
    /// Whether the user prefers to study on weekends.
    /// </summary>
    public bool WeekendStudy { get; set; } = true;

    /// <summary>
    /// Whether dark mode is enabled.
    /// </summary>
    public bool DarkMode { get; set; }

    /// <summary>
    /// Whether notifications are enabled.
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Pomodoro work duration in minutes.
    /// </summary>
    public int PomodoroWorkMinutes { get; set; } = 25;

    /// <summary>
    /// Pomodoro break duration in minutes.
    /// </summary>
    public int PomodoroBreakMinutes { get; set; } = 5;

    // Convenience properties
    [Ignore]
    public TimeSpan PreferredStart
    {
        get => TimeSpan.FromTicks(PreferredStartTicks);
        set => PreferredStartTicks = value.Ticks;
    }

    [Ignore]
    public TimeSpan PreferredEnd
    {
        get => TimeSpan.FromTicks(PreferredEndTicks);
        set => PreferredEndTicks = value.Ticks;
    }
}
