using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public class DailyTrendItem
{
    public string DayName { get; set; } = string.Empty;
    public double Hours { get; set; }
}

public class CourseDistributionItem
{
    public string CourseCode { get; set; } = string.Empty;
    public double Hours { get; set; }
}

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly IStatisticsService _statsService;
    private readonly IAuthService _authService;

    [ObservableProperty] private double _dailyHours;
    [ObservableProperty] private double _weeklyHours;
    [ObservableProperty] private double _monthlyHours;
    [ObservableProperty] private int _completedAssignments;
    [ObservableProperty] private int _totalAssignments;
    [ObservableProperty] private int _upcomingExams;
    [ObservableProperty] private int _studyStreak;
    [ObservableProperty] private double _avgSessionLength;
    [ObservableProperty] private double _productivityPercentage;

    public ObservableCollection<DailyTrendItem> WeeklyTrend { get; } = new();
    public ObservableCollection<CourseDistributionItem> CourseDistribution { get; } = new();

    public StatisticsViewModel(IStatisticsService statsService, IAuthService authService)
    {
        _statsService = statsService;
        _authService = authService;
        Title = "Productivity Analytics";
    }

    public async Task LoadStatisticsAsync()
    {
        IsBusy = true;
        var userId = _authService.CurrentUserId;

        DailyHours = await _statsService.GetDailyStudyHoursAsync(userId, DateTime.Now);
        WeeklyHours = await _statsService.GetWeeklyStudyHoursAsync(userId);
        MonthlyHours = await _statsService.GetMonthlyStudyHoursAsync(userId);
        CompletedAssignments = await _statsService.GetCompletedAssignmentsCountAsync(userId);
        TotalAssignments = await _statsService.GetTotalAssignmentsCountAsync(userId);
        UpcomingExams = await _statsService.GetUpcomingExamsCountAsync(userId);
        StudyStreak = await _statsService.GetStudyStreakAsync(userId);
        AvgSessionLength = await _statsService.GetAverageSessionLengthAsync(userId);
        ProductivityPercentage = await _statsService.GetProductivityPercentageAsync(userId);

        // Daily trend data
        var trend = await _statsService.GetDailyStudyHoursTrendAsync(userId, 7);
        WeeklyTrend.Clear();
        foreach (var kvp in trend)
        {
            WeeklyTrend.Add(new DailyTrendItem
            {
                DayName = kvp.Key.ToString("ddd"),
                Hours = kvp.Value
            });
        }

        // Course distribution
        var distribution = await _statsService.GetStudyHoursByCourseAsync(userId);
        CourseDistribution.Clear();
        foreach (var kvp in distribution)
        {
            CourseDistribution.Add(new CourseDistributionItem
            {
                CourseCode = kvp.Key,
                Hours = kvp.Value
            });
        }

        IsBusy = false;
    }
}
