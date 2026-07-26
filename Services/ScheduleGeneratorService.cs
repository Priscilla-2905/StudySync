using StudySync.Data;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.Services;

/// <summary>
/// Intelligent study schedule generator that analyzes timetable, assignments, exams,
/// and user preferences to create optimized study plans.
/// </summary>
public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly DatabaseService _db;

    public ScheduleGeneratorService(DatabaseService db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<List<StudySession>> GenerateScheduleAsync(int userId, int days = 7)
    {
        // Load all required data
        var preferences = await _db.GetPreferencesAsync(userId);
        var timetable = await _db.GetTimetableAsync(userId);
        var pendingAssignments = await _db.GetPendingAssignmentsAsync(userId);
        var upcomingExams = await _db.GetUpcomingExamsAsync(userId);
        var courses = await _db.GetCoursesAsync(userId);

        var courseDict = courses.ToDictionary(c => c.Id, c => c);
        var generatedSessions = new List<StudySession>();
        var startDate = DateTime.Now.Date;

        // Build prioritized work items
        var workItems = BuildPrioritizedWorkItems(pendingAssignments, upcomingExams, courseDict);

        if (workItems.Count == 0)
            return generatedSessions;

        // Generate schedule for each day
        for (int dayOffset = 0; dayOffset < days; dayOffset++)
        {
            var currentDate = startDate.AddDays(dayOffset);
            var dayOfWeek = GetStudyDay(currentDate.DayOfWeek);

            // Skip weekends if user doesn't prefer weekend study
            if (!preferences.WeekendStudy &&
                (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday))
                continue;

            // Get timetable entries for this day (busy blocks)
            var dayLectures = timetable
                .Where(t => t.Day == dayOfWeek)
                .OrderBy(t => t.StartTimeTicks)
                .ToList();

            // Find free time blocks within preferred study hours
            var freeBlocks = FindFreeBlocks(
                dayLectures,
                preferences.PreferredStart,
                preferences.PreferredEnd);

            // Track daily study hours
            double dailyHoursUsed = 0;

            // Fill free blocks with study sessions
            foreach (var block in freeBlocks)
            {
                if (dailyHoursUsed >= preferences.DailyLimit)
                    break;

                var sessions = FillBlock(
                    block,
                    workItems,
                    preferences,
                    currentDate,
                    userId,
                    ref dailyHoursUsed);

                generatedSessions.AddRange(sessions);
            }
        }

        // Save generated sessions to database
        foreach (var session in generatedSessions)
        {
            await _db.SaveAsync(session);
        }

        return generatedSessions;
    }

    /// <inheritdoc/>
    public async Task RegenerateScheduleAsync(int userId, int days = 7)
    {
        // Remove future incomplete sessions
        await _db.DeleteFutureStudySessionsAsync(userId, DateTime.Now.Date);

        // Generate new schedule
        await GenerateScheduleAsync(userId, days);
    }

    // ── Private Algorithm Methods ────────────────────────────────────

    /// <summary>
    /// Represents a schedulable work item with a priority score.
    /// </summary>
    private class WorkItem
    {
        public string Title { get; set; } = string.Empty;
        public int? AssignmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public double RemainingHours { get; set; }
        public double Score { get; set; }
        public bool IsExamPrep { get; set; }
    }

    /// <summary>
    /// Represents a free time block.
    /// </summary>
    private record TimeBlock(TimeSpan Start, TimeSpan End)
    {
        public double DurationMinutes => (End - Start).TotalMinutes;
    }

    /// <summary>
    /// Builds a prioritized list of work items from assignments and exams.
    /// Priority formula: Score = (1 / DaysUntilDeadline) × PriorityWeight × ImportanceFactor
    /// </summary>
    private static List<WorkItem> BuildPrioritizedWorkItems(
        List<Assignment> assignments,
        List<Exam> exams,
        Dictionary<int, Course> courses)
    {
        var items = new List<WorkItem>();

        // Add assignments as work items
        foreach (var assignment in assignments)
        {
            var daysUntil = Math.Max(1, (assignment.Deadline.Date - DateTime.Now.Date).TotalDays);
            var priorityWeight = assignment.Priority switch
            {
                Priority.Critical => 4.0,
                Priority.High => 3.0,
                Priority.Medium => 2.0,
                Priority.Low => 1.0,
                _ => 1.0
            };

            var courseName = courses.TryGetValue(assignment.CourseId, out var course)
                ? course.CourseName : "Unknown";

            items.Add(new WorkItem
            {
                Title = $"{courseName}: {assignment.Title}",
                AssignmentId = assignment.Id,
                CourseId = assignment.CourseId,
                CourseName = courseName,
                Deadline = assignment.Deadline,
                RemainingHours = Math.Max(0.5, assignment.EstimatedHours),
                Score = (1.0 / daysUntil) * priorityWeight,
                IsExamPrep = false
            });
        }

        // Add exams as work items (exam preparation)
        foreach (var exam in exams)
        {
            var daysUntil = Math.Max(1, (exam.ExamDate.Date - DateTime.Now.Date).TotalDays);
            var importanceWeight = exam.Importance switch
            {
                Importance.High => 3.0,
                Importance.Medium => 2.0,
                Importance.Low => 1.0,
                _ => 1.0
            };

            var courseName = courses.TryGetValue(exam.CourseId, out var course)
                ? course.CourseName : "Unknown";

            // Estimate study hours based on days until exam and importance
            var estimatedHours = Math.Min(daysUntil * 1.5, 20) * (importanceWeight / 2.0);

            items.Add(new WorkItem
            {
                Title = $"Exam Prep: {courseName}",
                AssignmentId = null,
                CourseId = exam.CourseId,
                CourseName = courseName,
                Deadline = exam.ExamDate,
                RemainingHours = estimatedHours,
                Score = (1.0 / daysUntil) * importanceWeight * 1.5, // Exams get a 1.5x boost
                IsExamPrep = true
            });
        }

        // Sort by score descending (highest priority first)
        return items.OrderByDescending(i => i.Score).ToList();
    }

    /// <summary>
    /// Finds free time blocks around lectures within preferred study hours.
    /// </summary>
    private static List<TimeBlock> FindFreeBlocks(
        List<TimetableEntry> lectures,
        TimeSpan preferredStart,
        TimeSpan preferredEnd)
    {
        var blocks = new List<TimeBlock>();
        var busyBlocks = lectures
            .Select(l => new TimeBlock(l.StartTime, l.EndTime))
            .OrderBy(b => b.Start)
            .ToList();

        var currentStart = preferredStart;

        foreach (var busy in busyBlocks)
        {
            // If the busy block starts after our current position, we have free time
            if (busy.Start > currentStart && busy.Start <= preferredEnd)
            {
                var freeEnd = busy.Start < preferredEnd ? busy.Start : preferredEnd;
                if ((freeEnd - currentStart).TotalMinutes >= 15) // Minimum 15-minute block
                {
                    blocks.Add(new TimeBlock(currentStart, freeEnd));
                }
            }

            // Move current position past the busy block
            if (busy.End > currentStart)
                currentStart = busy.End;
        }

        // Add remaining time after last lecture
        if (currentStart < preferredEnd && (preferredEnd - currentStart).TotalMinutes >= 15)
        {
            blocks.Add(new TimeBlock(currentStart, preferredEnd));
        }

        return blocks;
    }

    /// <summary>
    /// Fills a free time block with study sessions and breaks.
    /// </summary>
    private static List<StudySession> FillBlock(
        TimeBlock block,
        List<WorkItem> workItems,
        UserPreference preferences,
        DateTime date,
        int userId,
        ref double dailyHoursUsed)
    {
        var sessions = new List<StudySession>();
        var sessionLength = TimeSpan.FromMinutes(preferences.SessionLength);
        var breakLength = TimeSpan.FromMinutes(preferences.BreakLength);
        var currentTime = block.Start;
        var isFirstSession = true;

        while (currentTime + sessionLength <= block.End && dailyHoursUsed < preferences.DailyLimit)
        {
            // Find the next work item to schedule
            var workItem = workItems.FirstOrDefault(w => w.RemainingHours > 0);
            if (workItem is null)
                break;

            // Calculate actual session duration (may be shorter if limited by block end or daily limit)
            var remainingBlockTime = block.End - currentTime;
            var remainingDailyMinutes = (preferences.DailyLimit - dailyHoursUsed) * 60;
            var actualDuration = TimeSpan.FromMinutes(
                Math.Min(
                    Math.Min(sessionLength.TotalMinutes, remainingBlockTime.TotalMinutes),
                    remainingDailyMinutes));

            if (actualDuration.TotalMinutes < 15) // Skip if less than 15 minutes
                break;

            // Create study session
            sessions.Add(new StudySession
            {
                UserId = userId,
                AssignmentId = workItem.AssignmentId,
                CourseId = workItem.CourseId,
                Date = date,
                StartTime = currentTime,
                EndTime = currentTime + actualDuration,
                Completed = false,
                IsBreak = false,
                Title = workItem.Title
            });

            // Update tracking
            dailyHoursUsed += actualDuration.TotalHours;
            workItem.RemainingHours -= actualDuration.TotalHours;
            currentTime += actualDuration;

            // Add break if there's room for another session
            if (currentTime + breakLength + TimeSpan.FromMinutes(15) <= block.End &&
                dailyHoursUsed < preferences.DailyLimit)
            {
                sessions.Add(new StudySession
                {
                    UserId = userId,
                    CourseId = workItem.CourseId,
                    Date = date,
                    StartTime = currentTime,
                    EndTime = currentTime + breakLength,
                    Completed = false,
                    IsBreak = true,
                    Title = "Break"
                });

                currentTime += breakLength;
            }

            isFirstSession = false;
        }

        return sessions;
    }

    /// <summary>
    /// Converts System.DayOfWeek to our StudyDay enum.
    /// </summary>
    private static StudyDay GetStudyDay(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => StudyDay.Monday,
        DayOfWeek.Tuesday => StudyDay.Tuesday,
        DayOfWeek.Wednesday => StudyDay.Wednesday,
        DayOfWeek.Thursday => StudyDay.Thursday,
        DayOfWeek.Friday => StudyDay.Friday,
        DayOfWeek.Saturday => StudyDay.Saturday,
        DayOfWeek.Sunday => StudyDay.Sunday,
        _ => StudyDay.Monday
    };
}
