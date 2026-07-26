namespace StudySync.Services.Interfaces;

/// <summary>
/// Calculates and provides productivity statistics.
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Gets total study hours for a specific day.
    /// </summary>
    Task<double> GetDailyStudyHoursAsync(int userId, DateTime date);

    /// <summary>
    /// Gets total study hours for the current week.
    /// </summary>
    Task<double> GetWeeklyStudyHoursAsync(int userId);

    /// <summary>
    /// Gets total study hours for the current month.
    /// </summary>
    Task<double> GetMonthlyStudyHoursAsync(int userId);

    /// <summary>
    /// Gets the daily study hours for the past N days (for charting).
    /// </summary>
    Task<Dictionary<DateTime, double>> GetDailyStudyHoursTrendAsync(int userId, int days = 7);

    /// <summary>
    /// Gets the number of completed assignments.
    /// </summary>
    Task<int> GetCompletedAssignmentsCountAsync(int userId);

    /// <summary>
    /// Gets the total number of assignments.
    /// </summary>
    Task<int> GetTotalAssignmentsCountAsync(int userId);

    /// <summary>
    /// Gets the number of upcoming exams.
    /// </summary>
    Task<int> GetUpcomingExamsCountAsync(int userId);

    /// <summary>
    /// Gets the current study streak in days.
    /// </summary>
    Task<int> GetStudyStreakAsync(int userId);

    /// <summary>
    /// Gets the average study session length in minutes.
    /// </summary>
    Task<double> GetAverageSessionLengthAsync(int userId);

    /// <summary>
    /// Gets the productivity percentage (completed sessions / total sessions).
    /// </summary>
    Task<double> GetProductivityPercentageAsync(int userId);

    /// <summary>
    /// Gets study hours per course for pie chart.
    /// </summary>
    Task<Dictionary<string, double>> GetStudyHoursByCourseAsync(int userId);
}
