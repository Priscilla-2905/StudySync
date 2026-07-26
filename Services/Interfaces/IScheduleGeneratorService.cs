using StudySync.Models;

namespace StudySync.Services.Interfaces;

/// <summary>
/// Generates intelligent study schedules based on timetable, assignments, exams, and preferences.
/// </summary>
public interface IScheduleGeneratorService
{
    /// <summary>
    /// Generates a study schedule for the specified number of days.
    /// </summary>
    /// <param name="userId">The user to generate a schedule for.</param>
    /// <param name="days">Number of days to plan ahead (default 7).</param>
    /// <returns>List of generated study sessions.</returns>
    Task<List<StudySession>> GenerateScheduleAsync(int userId, int days = 7);

    /// <summary>
    /// Regenerates the schedule, removing future incomplete sessions first.
    /// </summary>
    Task RegenerateScheduleAsync(int userId, int days = 7);
}
