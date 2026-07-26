using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class PomodoroViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;
    private readonly IPreferenceService _prefService;
    private System.Timers.Timer? _timer;

    [ObservableProperty] private int _workMinutes = 25;
    [ObservableProperty] private int _breakMinutes = 5;
    [ObservableProperty] private int _secondsRemaining = 25 * 60;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isBreakMode;
    [ObservableProperty] private int _completedCycles;
    [ObservableProperty] private string _timerDisplay = "25:00";
    [ObservableProperty] private string _modeDisplay = "Focus Time";
    [ObservableProperty] private double _progress = 1.0;

    public PomodoroViewModel(Data.DatabaseService db, IAuthService authService, IPreferenceService prefService)
    {
        _db = db;
        _authService = authService;
        _prefService = prefService;
        Title = "Pomodoro Timer";
    }

    public async Task InitializeAsync()
    {
        var prefs = await _prefService.GetPreferencesAsync(_authService.CurrentUserId);
        WorkMinutes = prefs.PomodoroWorkMinutes;
        BreakMinutes = prefs.PomodoroBreakMinutes;
        ResetTimer();
    }

    [RelayCommand]
    private void StartTimer()
    {
        if (IsRunning) return;

        IsRunning = true;
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    [RelayCommand]
    private void PauseTimer()
    {
        IsRunning = false;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    [RelayCommand]
    private void ResetTimer()
    {
        PauseTimer();
        IsBreakMode = false;
        SecondsRemaining = WorkMinutes * 60;
        ModeDisplay = "Focus Time";
        UpdateDisplay();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (SecondsRemaining > 0)
            {
                SecondsRemaining--;
                UpdateDisplay();
            }
            else
            {
                PauseTimer();
                if (!IsBreakMode)
                {
                    // Completed a work cycle
                    CompletedCycles++;
                    IsBreakMode = true;
                    SecondsRemaining = BreakMinutes * 60;
                    ModeDisplay = "Break Time ☕";
                    await SavePomodoroSessionAsync();
                    await Shell.Current.DisplayAlert("Pomodoro Complete!", "Great job! Take a well-deserved break.", "Start Break");
                }
                else
                {
                    IsBreakMode = false;
                    SecondsRemaining = WorkMinutes * 60;
                    ModeDisplay = "Focus Time";
                    await Shell.Current.DisplayAlert("Break Finished!", "Ready to focus again?", "Start Focus");
                }
                UpdateDisplay();
            }
        });
    }

    private void UpdateDisplay()
    {
        int minutes = SecondsRemaining / 60;
        int seconds = SecondsRemaining % 60;
        TimerDisplay = $"{minutes:D2}:{seconds:D2}";

        int totalSeconds = (IsBreakMode ? BreakMinutes : WorkMinutes) * 60;
        Progress = totalSeconds > 0 ? (double)SecondsRemaining / totalSeconds : 0;
    }

    private async Task SavePomodoroSessionAsync()
    {
        var session = new PomodoroSession
        {
            UserId = _authService.CurrentUserId,
            Date = DateTime.Now,
            WorkDurationMinutes = WorkMinutes,
            BreakDurationMinutes = BreakMinutes,
            CompletedCycles = 1,
            TotalMinutes = WorkMinutes
        };
        await _db.SaveAsync(session);
    }
}
