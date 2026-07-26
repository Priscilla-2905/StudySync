using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents a student assignment with tracking information.
/// </summary>
[Table("Assignments")]
public class Assignment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [Indexed]
    public int CourseId { get; set; }

    [MaxLength(200), NotNull]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>
    /// Estimated number of hours to complete this assignment.
    /// </summary>
    public double EstimatedHours { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [Ignore]
    public Course? Course { get; set; }

    // Computed properties
    [Ignore]
    public bool IsOverdue => Status != AssignmentStatus.Completed && Deadline < DateTime.Now;

    [Ignore]
    public int DaysRemaining => Math.Max(0, (int)(Deadline.Date - DateTime.Now.Date).TotalDays);

    [Ignore]
    public string StatusDisplay => Status switch
    {
        AssignmentStatus.Pending => "Pending",
        AssignmentStatus.InProgress => "In Progress",
        AssignmentStatus.Completed => "Completed",
        _ => "Unknown"
    };
}
