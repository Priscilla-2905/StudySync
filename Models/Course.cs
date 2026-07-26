using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents an academic course.
/// </summary>
[Table("Courses")]
public class Course
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [MaxLength(20), NotNull]
    public string CourseCode { get; set; } = string.Empty;

    [MaxLength(150), NotNull]
    public string CourseName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Lecturer { get; set; } = string.Empty;

    public int Credits { get; set; }

    /// <summary>
    /// Hex colour code for visual identification (e.g., "#6C63FF").
    /// </summary>
    [MaxLength(10)]
    public string Colour { get; set; } = "#6C63FF";
}
