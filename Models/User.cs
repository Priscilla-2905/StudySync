using SQLite;

namespace StudySync.Models;

/// <summary>
/// Represents a registered user in the system.
/// </summary>
[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(150), NotNull, Unique]
    public string Email { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Salt { get; set; } = string.Empty;

    [MaxLength(50)]
    public string StudentId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Program { get; set; } = string.Empty;

    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
