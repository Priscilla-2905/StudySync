using StudySync.Data;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.Services;

/// <summary>
/// Provides productivity calculations, streak tracking, study statistics, and dashboard summaries.
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly DatabaseService _db;

    public StatisticsService(DatabaseService db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<double> GetDailyStudyHoursAsync(int userId, DateTime date)
    {
        var sessions = await _db.GetStudySessionsAsync(userId, date);
        return sessions
            .Where(s => s.Completed && !s.IsBreak)
            .Sum(s => s.DurationHours);
    }

    /// <inheritdoc/>
    public async Task<double> GetWeeklyStudyHoursAsync(int userId)
    {
        var today = DateTime.Now.Date;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = today.AddDays(-diff);
        var endOfWeek = startOfWeek.AddDays(6);

        var sessions = await _db.GetStudySessionsRangeAsync(userId, startOfWeek, endOfWeek);
        return sessions
            .Where(s => s.Completed && !s.IsBreak)
            .Sum(s => s.DurationHours);
    }

    /// <inheritdoc/>
    public async Task<double> GetMonthlyStudyHoursAsync(int userId)
    {
        var today = DateTime.Now.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var sessions = await _db.GetStudySessionsRangeAsync(userId, startOfMonth, endOfMonth);
        return sessions
            .Where(s => s.Completed && !s.IsBreak)
            .Sum(s => s.DurationHours);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DateTime, double>> GetDailyStudyHoursTrendAsync(int userId, int days = 7)
    {
        var result = new Dictionary<DateTime, double>();
        var endDate = DateTime.Now.Date;
        var startDate = endDate.AddDays(-(days - 1));

        var sessions = await _db.GetStudySessionsRangeAsync(userId, startDate, endDate);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var targetDate = date;
            var hours = sessions
                .Where(s => s.Date.Date == targetDate && s.Completed && !s.IsBreak)
                .Sum(s => s.DurationHours);
            result[targetDate] = Math.Round(hours, 1);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<int> GetCompletedAssignmentsCountAsync(int userId)
    {
        var assignments = await _db.GetAssignmentsAsync(userId);
        return assignments.Count(a => a.Status == AssignmentStatus.Completed);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalAssignmentsCountAsync(int userId)
    {
        var assignments = await _db.GetAssignmentsAsync(userId);
        return assignments.Count;
    }

    /// <inheritdoc/>
    public async Task<int> GetUpcomingExamsCountAsync(int userId)
    {
        var exams = await _db.GetUpcomingExamsAsync(userId);
        return exams.Count;
    }

    /// <inheritdoc/>
    public async Task<int> GetStudyStreakAsync(int userId)
    {
        var sessions = await _db.GetAllStudySessionsAsync(userId);
        var completedDates = sessions
            .Where(s => s.Completed && !s.IsBreak)
            .Select(s => s.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (completedDates.Count == 0)
            return 0;

        int streak = 0;
        var checkDate = DateTime.Now.Date;

        // If today has no study session completed yet, start check from yesterday
        if (!completedDates.Contains(checkDate))
        {
            checkDate = checkDate.AddDays(-1);
        }

        while (completedDates.Contains(checkDate))
        {
            streak++;
            checkDate = checkDate.AddDays(-1);
        }

        return streak;
    }

    /// <inheritdoc/>
    public async Task<double> GetAverageSessionLengthAsync(int userId)
    {
        var sessions = await _db.GetAllStudySessionsAsync(userId);
        var completedSessions = sessions.Where(s => s.Completed && !s.IsBreak).ToList();

        if (completedSessions.Count == 0) return 0;

        return Math.Round(completedSessions.Average(s => s.DurationMinutes), 1);
    }

    /// <inheritdoc/>
    public async Task<double> GetProductivityPercentageAsync(int userId)
    {
        var today = DateTime.Now.Date;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = today.AddDays(-diff);

        var sessions = await _db.GetStudySessionsRangeAsync(userId, startOfWeek, today);
        var nonBreakSessions = sessions.Where(s => !s.IsBreak && s.Date.Date <= today).ToList();

        if (nonBreakSessions.Count == 0) return 100.0;

        var completedCount = nonBreakSessions.Count(s => s.Completed);
        return Math.Round(((double)completedCount / nonBreakSessions.Count) * 100.0, 1);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, double>> GetStudyHoursByCourseAsync(int userId)
    {
        var courses = await _db.GetCoursesAsync(userId);
        var courseDict = courses.ToDictionary(c => c.Id, c => c.CourseCode);
        var sessions = await _db.GetAllStudySessionsAsync(userId);

        var result = new Dictionary<string, double>();

        foreach (var group in sessions.Where(s => s.Completed && !s.IsBreak).GroupBy(s => s.CourseId))
        {
            var courseCode = courseDict.TryGetValue(group.Key, out var code) ? code : "Other";
            var totalHours = group.Sum(s => s.DurationHours);
            result[courseCode] = Math.Round(totalHours, 1);
        }

        return result;
    }
}
