namespace StudySync.Models;

/// <summary>
/// Defines assignment completion statuses.
/// </summary>
public enum AssignmentStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}

/// <summary>
/// Defines priority levels for assignments and tasks.
/// </summary>
public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Defines importance levels for exams.
/// </summary>
public enum Importance
{
    Low = 0,
    Medium = 1,
    High = 2
}

/// <summary>
/// Defines days of the week for timetable entries.
/// </summary>
public enum StudyDay
{
    Monday = 0,
    Tuesday = 1,
    Wednesday = 2,
    Thursday = 3,
    Friday = 4,
    Saturday = 5,
    Sunday = 6
}

/// <summary>
/// Defines types of related entities for todo items.
/// </summary>
public enum RelatedEntityType
{
    None = 0,
    Assignment = 1,
    Exam = 2,
    StudySession = 3,
    Lecture = 4
}
