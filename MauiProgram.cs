using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using StudySync.Data;
using StudySync.Services;
using StudySync.Services.Interfaces;
using IAppNotificationService = StudySync.Services.Interfaces.INotificationService;
using StudySync.ViewModels;
using StudySync.Views;

namespace StudySync;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Data & Repositories
        builder.Services.AddSingleton<DatabaseService>();

        // Business Logic Services
        builder.Services.AddSingleton<IAuthService, AuthenticationService>();
        builder.Services.AddSingleton<IScheduleGeneratorService, ScheduleGeneratorService>();
        builder.Services.AddSingleton<IAppNotificationService, NotificationService>();
        builder.Services.AddSingleton<IExportService, ExportService>();
        builder.Services.AddSingleton<IPreferenceService, PreferenceService>();
        builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<CoursesViewModel>();
        builder.Services.AddTransient<CourseDetailViewModel>();
        builder.Services.AddTransient<TimetableViewModel>();
        builder.Services.AddTransient<TimetableEntryViewModel>();
        builder.Services.AddTransient<AssignmentsViewModel>();
        builder.Services.AddTransient<AssignmentDetailViewModel>();
        builder.Services.AddTransient<ExamsViewModel>();
        builder.Services.AddTransient<ExamDetailViewModel>();
        builder.Services.AddTransient<StudyScheduleViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<TodoViewModel>();
        builder.Services.AddTransient<PomodoroViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<ExportViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<CoursesPage>();
        builder.Services.AddTransient<CourseDetailPage>();
        builder.Services.AddTransient<TimetablePage>();
        builder.Services.AddTransient<TimetableEntryPage>();
        builder.Services.AddTransient<AssignmentsPage>();
        builder.Services.AddTransient<AssignmentDetailPage>();
        builder.Services.AddTransient<ExamsPage>();
        builder.Services.AddTransient<ExamDetailPage>();
        builder.Services.AddTransient<StudySchedulePage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<TodoPage>();
        builder.Services.AddTransient<PomodoroPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<ExportPage>();

        return builder.Build();
    }
}
