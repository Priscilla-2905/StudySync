using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class StudyScheduleViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IScheduleGeneratorService _scheduleService;
    private readonly IAuthService _authService;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Now.Date;

    public ObservableCollection<StudySession> Sessions { get; } = new();

    public StudyScheduleViewModel(
        Data.DatabaseService db,
        IScheduleGeneratorService scheduleService,
        IAuthService authService)
    {
        _db = db;
        _scheduleService = scheduleService;
        _authService = authService;
        Title = "Study Schedule";
    }

    public async Task LoadSessionsAsync()
    {
        IsBusy = true;
        Sessions.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetStudySessionsAsync(userId, SelectedDate);
        var courses = (await _db.GetCoursesAsync(userId)).ToDictionary(c => c.Id, c => c);

        foreach (var session in list)
        {
            session.Course = courses.TryGetValue(session.CourseId, out var c) ? c : null;
            Sessions.Add(session);
        }
        IsBusy = false;
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = LoadSessionsAsync();
    }

    [RelayCommand]
    private async Task RegenerateScheduleAsync()
    {
        IsBusy = true;
        var userId = _authService.CurrentUserId;
        await _scheduleService.RegenerateScheduleAsync(userId);
        await LoadSessionsAsync();
        IsBusy = false;
        await Shell.Current.DisplayAlert("Schedule Updated", "Smart schedule successfully regenerated!", "OK");
    }

    [RelayCommand]
    private async Task ToggleSessionCompleteAsync(StudySession session)
    {
        if (session is null) return;
        session.Completed = !session.Completed;
        await _db.SaveAsync(session);
    }
}
