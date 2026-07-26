using SQLite;

namespace StudySync.Models;

/// <summary>
/// Tracks completed Pomodoro sessions for productivity statistics.
/// </summary>
[Table("PomodoroSessions")]
public class PomodoroSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Work duration in minutes for this Pomodoro cycle.
    /// </summary>
    public int WorkDurationMinutes { get; set; } = 25;

    /// <summary>
    /// Break duration in minutes for this Pomodoro cycle.
    /// </summary>
    public int BreakDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Number of completed work cycles in this session.
    /// </summary>
    public int CompletedCycles { get; set; }

    /// <summary>
    /// Total productive minutes in this session.
    /// </summary>
    public double TotalMinutes { get; set; }

    /// <summary>
    /// Optional course this Pomodoro was associated with.
    /// </summary>
    public int? CourseId { get; set; }

    [Ignore]
    public Course? Course { get; set; }
}
