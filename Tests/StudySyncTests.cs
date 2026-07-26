using StudySync.Data;
using StudySync.Helpers;
using StudySync.Models;
using StudySync.Services;
using Xunit;

namespace StudySync.Tests;

public class ValidationHelperTests
{
    [Theory]
    [InlineData("student@university.edu", true)]
    [InlineData("test.user@domain.co.uk", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_ValidatesCorrectly(string email, bool expected)
    {
        var result = ValidationHelper.IsValidEmail(email);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123456", true)]
    [InlineData("password123", true)]
    [InlineData("12345", false)]
    [InlineData("", false)]
    public void IsValidPassword_ValidatesLength(string password, bool expected)
    {
        var isValid = ValidationHelper.IsValidPassword(password, out var err);
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void DoTimesOverlap_DetectsOverlapsCorrectly()
    {
        var t9to11_Start = TimeSpan.FromHours(9);
        var t9to11_End = TimeSpan.FromHours(11);

        var t10to12_Start = TimeSpan.FromHours(10);
        var t10to12_End = TimeSpan.FromHours(12);

        var t11to13_Start = TimeSpan.FromHours(11);
        var t11to13_End = TimeSpan.FromHours(13);

        // 9-11 and 10-12 overlap
        Assert.True(ValidationHelper.DoTimesOverlap(t9to11_Start, t9to11_End, t10to12_Start, t10to12_End));

        // 9-11 and 11-13 do NOT overlap (adjacent)
        Assert.False(ValidationHelper.DoTimesOverlap(t9to11_Start, t9to11_End, t11to13_Start, t11to13_End));
    }
}

public class IntegrationAndAlgorithmTests
{
    private DatabaseService CreateTestDatabase()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"StudySyncTest_{Guid.NewGuid():N}.db3");
        return new DatabaseService(dbPath);
    }

    [Fact]
    public async Task RegistrationAndLogin_FlowWorks()
    {
        var db = CreateTestDatabase();
        var authService = new AuthenticationService(db);

        var email = $"user_{Guid.NewGuid():N}@university.edu";
        var password = "SecurePassword123";

        // 1. Register User
        var (regSuccess, regMsg) = await authService.RegisterAsync("Test Student", email, password, password);
        Assert.True(regSuccess, regMsg);

        // 2. Reject Duplicate Registration
        var (dupSuccess, _) = await authService.RegisterAsync("Test Student 2", email, password, password);
        Assert.False(dupSuccess);

        // 3. Login with Wrong Password -> Fail
        var (wrongPassSuccess, _) = await authService.LoginAsync(email, "WrongPassword", false);
        Assert.False(wrongPassSuccess);

        // 4. Login with Correct Password -> Success
        var (loginSuccess, loginMsg) = await authService.LoginAsync(email, password, false);
        Assert.True(loginSuccess, loginMsg);
        Assert.True(authService.IsLoggedIn);
        Assert.True(authService.CurrentUserId > 0);
    }

    [Fact]
    public async Task IntelligentScheduleGenerator_CreatesValidSessionsWithoutConflicts()
    {
        var db = CreateTestDatabase();
        var authService = new AuthenticationService(db);
        var scheduleService = new ScheduleGeneratorService(db);

        var email = $"sched_user_{Guid.NewGuid():N}@university.edu";
        var (_, _) = await authService.RegisterAsync("Schedule Student", email, "password123", "password123");
        await authService.LoginAsync(email, "password123", false);
        var userId = authService.CurrentUserId;

        // Create Course
        var course = new Course
        {
            UserId = userId,
            CourseCode = "CS101",
            CourseName = "Data Structures",
            Lecturer = "Dr. Alan",
            Credits = 3,
            Colour = "#6C63FF"
        };
        await db.SaveAsync(course);

        // Create Timetable Lecture: Monday 9:00 AM - 11:00 AM
        var lecture = new TimetableEntry
        {
            UserId = userId,
            CourseId = course.Id,
            Day = StudyDay.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(11),
            Venue = "Hall A",
            Lecturer = "Dr. Alan"
        };
        await db.SaveAsync(lecture);

        // Create High Priority Assignment due in 2 days
        var assignment = new Assignment
        {
            UserId = userId,
            CourseId = course.Id,
            Title = "Binary Search Tree Implementation",
            Deadline = DateTime.Now.AddDays(2),
            Priority = Priority.Critical,
            EstimatedHours = 3.0,
            Status = AssignmentStatus.Pending
        };
        await db.SaveAsync(assignment);

        // Set Preferences: Preferred 8:00 AM to 10:00 PM, 3.0 hr daily limit, 60min sessions, 15min breaks
        var prefs = await db.GetPreferencesAsync(userId);
        prefs.PreferredStart = TimeSpan.FromHours(8);
        prefs.PreferredEnd = TimeSpan.FromHours(22);
        prefs.DailyLimit = 3.0;
        prefs.SessionLength = 60;
        prefs.BreakLength = 15;
        await db.SaveAsync(prefs);

        // Generate Schedule
        var sessions = await scheduleService.GenerateScheduleAsync(userId, days: 7);

        Assert.NotEmpty(sessions);

        // Verify no study session overlaps with Monday lecture (9 AM - 11 AM)
        var mondaySessions = sessions.Where(s => s.Date.DayOfWeek == DayOfWeek.Monday).ToList();
        foreach (var session in mondaySessions)
        {
            var overlapsWithLecture = ValidationHelper.DoTimesOverlap(
                session.StartTime, session.EndTime,
                lecture.StartTime, lecture.EndTime);

            Assert.False(overlapsWithLecture, $"Study session {session.StartTime}-{session.EndTime} overlaps with lecture 9:00-11:00");
        }
    }

    [Fact]
    public async Task StatisticsService_CalculatesMetricsCorrectly()
    {
        var db = CreateTestDatabase();
        var authService = new AuthenticationService(db);
        var statsService = new StatisticsService(db);

        var email = $"stats_user_{Guid.NewGuid():N}@university.edu";
        await authService.RegisterAsync("Stats Student", email, "password123", "password123");
        await authService.LoginAsync(email, "password123", false);
        var userId = authService.CurrentUserId;

        // Add a course
        var course = new Course { UserId = userId, CourseCode = "MATH101", CourseName = "Calculus", Credits = 4 };
        await db.SaveAsync(course);

        // Add completed study session for today (2 hours)
        var session = new StudySession
        {
            UserId = userId,
            CourseId = course.Id,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromHours(14),
            EndTime = TimeSpan.FromHours(16),
            Completed = true,
            IsBreak = false,
            Title = "Calculus Derivatives"
        };
        await db.SaveAsync(session);

        // Verify Daily & Weekly Study Hours
        var dailyHours = await statsService.GetDailyStudyHoursAsync(userId, DateTime.Now.Date);
        Assert.Equal(2.0, dailyHours);

        var weeklyHours = await statsService.GetWeeklyStudyHoursAsync(userId);
        Assert.True(weeklyHours >= 2.0);

        // Verify Study Streak
        var streak = await statsService.GetStudyStreakAsync(userId);
        Assert.True(streak >= 1);
    }
}
