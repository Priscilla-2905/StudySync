using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IStatisticsService _statsService;
    private readonly IScheduleGeneratorService _scheduleService;
    private readonly Data.DatabaseService _db;

    [ObservableProperty] private string _welcomeMessage = "Welcome!";
    [ObservableProperty] private double _hoursStudiedThisWeek;
    [ObservableProperty] private int _assignmentsCompleted;
    [ObservableProperty] private int _totalAssignments;
    [ObservableProperty] private int _upcomingExamsCount;
    [ObservableProperty] private double _productivityPercentage;
    [ObservableProperty] private int _studyStreak;

    public ObservableCollection<TimetableEntry> TodayLectures { get; } = new();
    public ObservableCollection<Assignment> TodayAssignments { get; } = new();
    public ObservableCollection<Exam> UpcomingExams { get; } = new();
    public ObservableCollection<StudySession> TodaySessions { get; } = new();

    public DashboardViewModel(
        IAuthService authService,
        IStatisticsService statsService,
        IScheduleGeneratorService scheduleService,
        Data.DatabaseService db)
    {
        _authService = authService;
        _statsService = statsService;
        _scheduleService = scheduleService;
        _db = db;
        Title = "Dashboard";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;

        var user = await _authService.GetCurrentUserAsync();
        if (user is null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        WelcomeMessage = $"Welcome back, {user.FullName.Split(' ')[0]}! 👋";

        var userId = user.Id;

        // Statistics
        HoursStudiedThisWeek = await _statsService.GetWeeklyStudyHoursAsync(userId);
        AssignmentsCompleted = await _statsService.GetCompletedAssignmentsCountAsync(userId);
        TotalAssignments = await _statsService.GetTotalAssignmentsCountAsync(userId);
        UpcomingExamsCount = await _statsService.GetUpcomingExamsCountAsync(userId);
        ProductivityPercentage = await _statsService.GetProductivityPercentageAsync(userId);
        StudyStreak = await _statsService.GetStudyStreakAsync(userId);

        // Today's Lectures
        var dayOfWeek = (StudyDay)((int)DateTime.Now.DayOfWeek == 0 ? 6 : (int)DateTime.Now.DayOfWeek - 1);
        var lectures = await _db.GetTimetableForDayAsync(userId, dayOfWeek);
        TodayLectures.Clear();
        foreach (var l in lectures)
        {
            l.Course = await _db.GetByIdAsync<Course>(l.CourseId);
            TodayLectures.Add(l);
        }

        // Today's Assignments Due
        var pending = await _db.GetPendingAssignmentsAsync(userId);
        TodayAssignments.Clear();
        foreach (var a in pending.Where(a => a.Deadline.Date == DateTime.Now.Date))
        {
            a.Course = await _db.GetByIdAsync<Course>(a.CourseId);
            TodayAssignments.Add(a);
        }

        // Upcoming Exams
        var exams = await _db.GetUpcomingExamsAsync(userId);
        UpcomingExams.Clear();
        foreach (var e in exams.Take(3))
        {
            e.Course = await _db.GetByIdAsync<Course>(e.CourseId);
            UpcomingExams.Add(e);
        }

        // Today's Study Sessions
        var sessions = await _db.GetStudySessionsAsync(userId, DateTime.Now);
        TodaySessions.Clear();
        foreach (var s in sessions)
        {
            s.Course = await _db.GetByIdAsync<Course>(s.CourseId);
            TodaySessions.Add(s);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GenerateScheduleAsync()
    {
        IsBusy = true;
        var userId = _authService.CurrentUserId;
        await _scheduleService.RegenerateScheduleAsync(userId);
        await InitializeAsync();
        await Shell.Current.DisplayAlert("Schedule Generated", "Your study schedule has been updated!", "OK");
    }

    [RelayCommand]
    private async Task ToggleSessionCompleteAsync(StudySession session)
    {
        if (session is null) return;
        session.Completed = !session.Completed;
        await _db.SaveAsync(session);
        await InitializeAsync();
    }
}
