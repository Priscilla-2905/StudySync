using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents an upcoming exam.
/// </summary>
[Table("Exams")]
public class Exam
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [Indexed]
    public int CourseId { get; set; }

    public DateTime ExamDate { get; set; }

    /// <summary>
    /// Exam time stored as ticks for SQLite compatibility.
    /// </summary>
    public long TimeTicks { get; set; }

    [MaxLength(100)]
    public string Venue { get; set; } = string.Empty;

    public Importance Importance { get; set; } = Importance.Medium;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Convenience property
    [Ignore]
    public TimeSpan Time
    {
        get => TimeSpan.FromTicks(TimeTicks);
        set => TimeTicks = value.Ticks;
    }

    // Navigation property
    [Ignore]
    public Course? Course { get; set; }

    // Computed properties
    [Ignore]
    public int DaysUntilExam => Math.Max(0, (int)(ExamDate.Date - DateTime.Now.Date).TotalDays);

    [Ignore]
    public string CountdownDisplay
    {
        get
        {
            var days = DaysUntilExam;
            return days switch
            {
                0 => "Today!",
                1 => "Tomorrow",
                _ when days <= 7 => $"{days} days",
                _ => $"{days} days ({ExamDate:MMM dd})"
            };
        }
    }

    [Ignore]
    public bool IsPast => ExamDate.Date < DateTime.Now.Date;
}
