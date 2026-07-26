using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public class CalendarEventItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string TimeDisplay { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // Lecture, Assignment, Exam, StudySession
    public string Color { get; set; } = "#6C63FF";
}

public partial class CalendarViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Now.Date;

    public ObservableCollection<CalendarEventItem> DayEvents { get; } = new();

    public CalendarViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Calendar";
    }

    public async Task LoadDateEventsAsync()
    {
        IsBusy = true;
        DayEvents.Clear();
        var userId = _authService.CurrentUserId;
        var date = SelectedDate.Date;
        var courses = (await _db.GetCoursesAsync(userId)).ToDictionary(c => c.Id, c => c);

        // 1. Lectures for this day of week
        var studyDay = (StudyDay)((int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1);
        var lectures = await _db.GetTimetableForDayAsync(userId, studyDay);
        foreach (var l in lectures)
        {
            var course = courses.TryGetValue(l.CourseId, out var c) ? c : null;
            DayEvents.Add(new CalendarEventItem
            {
                Title = $"Lecture: {course?.CourseCode ?? "Course"}",
                Subtitle = $"Venue: {l.Venue}",
                TimeDisplay = $"{l.StartTime:hh\\:mm} - {l.EndTime:hh\\:mm}",
                EventType = "Lecture",
                Color = course?.Colour ?? "#6C63FF"
            });
        }

        // 2. Assignments due on this date
        var assignments = await _db.GetAssignmentsAsync(userId);
        foreach (var a in assignments.Where(a => a.Deadline.Date == date))
        {
            var course = courses.TryGetValue(a.CourseId, out var c) ? c : null;
            DayEvents.Add(new CalendarEventItem
            {
                Title = $"Assignment Due: {a.Title}",
                Subtitle = $"Course: {course?.CourseCode ?? "Unknown"}",
                TimeDisplay = $"{a.Deadline:hh\\:mm tt}",
                EventType = "Assignment",
                Color = "#FF6584"
            });
        }

        // 3. Exams on this date
        var exams = await _db.GetExamsAsync(userId);
        foreach (var e in exams.Where(e => e.ExamDate.Date == date))
        {
            var course = courses.TryGetValue(e.CourseId, out var c) ? c : null;
            DayEvents.Add(new CalendarEventItem
            {
                Title = $"EXAM: {course?.CourseCode ?? "Course"}",
                Subtitle = $"Venue: {e.Venue}",
                TimeDisplay = $"{e.Time:hh\\:mm}",
                EventType = "Exam",
                Color = "#F44336"
            });
        }

        // 4. Study Sessions on this date
        var sessions = await _db.GetStudySessionsAsync(userId, date);
        foreach (var s in sessions)
        {
            var course = courses.TryGetValue(s.CourseId, out var c) ? c : null;
            DayEvents.Add(new CalendarEventItem
            {
                Title = s.IsBreak ? "Break" : $"Study: {s.Title}",
                Subtitle = course?.CourseCode ?? string.Empty,
                TimeDisplay = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}",
                EventType = s.IsBreak ? "Break" : "StudySession",
                Color = s.IsBreak ? "#888888" : (course?.Colour ?? "#4CAF50")
            });
        }

        IsBusy = false;
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = LoadDateEventsAsync();
    }
}
