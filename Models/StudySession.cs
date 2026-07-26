using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents a planned or completed study session.
/// </summary>
[Table("StudySessions")]
public class StudySession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    /// <summary>
    /// Optional link to an assignment this session is for.
    /// </summary>
    public int? AssignmentId { get; set; }

    [Indexed]
    public int CourseId { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Session start time stored as ticks.
    /// </summary>
    public long StartTimeTicks { get; set; }

    /// <summary>
    /// Session end time stored as ticks.
    /// </summary>
    public long EndTimeTicks { get; set; }

    public bool Completed { get; set; }

    /// <summary>
    /// Whether this is a break session between study blocks.
    /// </summary>
    public bool IsBreak { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Convenience properties
    [Ignore]
    public TimeSpan StartTime
    {
        get => TimeSpan.FromTicks(StartTimeTicks);
        set => StartTimeTicks = value.Ticks;
    }

    [Ignore]
    public TimeSpan EndTime
    {
        get => TimeSpan.FromTicks(EndTimeTicks);
        set => EndTimeTicks = value.Ticks;
    }

    [Ignore]
    public double DurationMinutes => (EndTime - StartTime).TotalMinutes;

    [Ignore]
    public double DurationHours => (EndTime - StartTime).TotalHours;

    // Navigation properties
    [Ignore]
    public Course? Course { get; set; }

    [Ignore]
    public Assignment? Assignment { get; set; }
}
