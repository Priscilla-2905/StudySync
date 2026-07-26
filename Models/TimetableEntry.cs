using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents a single lecture entry in the weekly timetable.
/// </summary>
[Table("Timetable")]
public class TimetableEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [Indexed]
    public int CourseId { get; set; }

    /// <summary>
    /// Day of the week for this lecture.
    /// </summary>
    public StudyDay Day { get; set; }

    /// <summary>
    /// Lecture start time stored as ticks for SQLite compatibility.
    /// </summary>
    public long StartTimeTicks { get; set; }

    /// <summary>
    /// Lecture end time stored as ticks for SQLite compatibility.
    /// </summary>
    public long EndTimeTicks { get; set; }

    [MaxLength(100)]
    public string Venue { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Lecturer { get; set; } = string.Empty;

    // Convenience properties (not stored in DB)
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

    /// <summary>
    /// Navigation property - populated at runtime.
    /// </summary>
    [Ignore]
    public Course? Course { get; set; }
}
