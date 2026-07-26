using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IPreferenceService _prefService;

    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _studentId = string.Empty;
    [ObservableProperty] private string _program = string.Empty;
    [ObservableProperty] private string _academicYear = string.Empty;

    // Study Preferences
    [ObservableProperty] private TimeSpan _preferredStart = TimeSpan.FromHours(8);
    [ObservableProperty] private TimeSpan _preferredEnd = TimeSpan.FromHours(22);
    [ObservableProperty] private double _dailyLimit = 4.0;
    [ObservableProperty] private int _sessionLength = 60;
    [ObservableProperty] private int _breakLength = 15;
    [ObservableProperty] private bool _weekendStudy = true;
    [ObservableProperty] private bool _darkMode;
    [ObservableProperty] private bool _notificationsEnabled = true;

    [ObservableProperty] private string _statusMessage = string.Empty;

    private User? _user;
    private UserPreference? _preference;

    public ProfileViewModel(IAuthService authService, IPreferenceService prefService)
    {
        _authService = authService;
        _prefService = prefService;
        Title = "User Profile";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        _user = await _authService.GetCurrentUserAsync();
        if (_user is not null)
        {
            FullName = _user.FullName;
            Email = _user.Email;
            StudentId = _user.StudentId;
            Program = _user.Program;
            AcademicYear = _user.AcademicYear;

            _preference = await _prefService.GetPreferencesAsync(_user.Id);
            PreferredStart = _preference.PreferredStart;
            PreferredEnd = _preference.PreferredEnd;
            DailyLimit = _preference.DailyLimit;
            SessionLength = _preference.SessionLength;
            BreakLength = _preference.BreakLength;
            WeekendStudy = _preference.WeekendStudy;
            DarkMode = _preference.DarkMode;
            NotificationsEnabled = _preference.NotificationsEnabled;
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || _user is null || _preference is null) return;

        IsBusy = true;
        StatusMessage = string.Empty;

        _user.FullName = FullName;
        _user.StudentId = StudentId;
        _user.Program = Program;
        _user.AcademicYear = AcademicYear;

        var (success, msg) = await _authService.UpdateProfileAsync(_user);

        if (success)
        {
            _preference.PreferredStart = PreferredStart;
            _preference.PreferredEnd = PreferredEnd;
            _preference.DailyLimit = DailyLimit;
            _preference.SessionLength = SessionLength;
            _preference.BreakLength = BreakLength;
            _preference.WeekendStudy = WeekendStudy;
            _preference.DarkMode = DarkMode;
            _preference.NotificationsEnabled = NotificationsEnabled;

            await _prefService.SavePreferencesAsync(_preference);
            StatusMessage = "Profile and preferences saved successfully!";
        }
        else
        {
            StatusMessage = msg;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
