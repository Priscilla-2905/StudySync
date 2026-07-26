using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class TimetableViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private StudyDay _selectedDay = StudyDay.Monday;
    [ObservableProperty] private bool _isGridView = false;

    public ObservableCollection<TimetableEntry> Entries { get; } = new();
    public List<StudyDay> DaysOfWeek { get; } = Enum.GetValues<StudyDay>().ToList();

    public TimetableViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Timetable";
    }

    public async Task LoadTimetableAsync()
    {
        IsBusy = true;
        Entries.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetTimetableForDayAsync(userId, SelectedDay);
        foreach (var entry in list)
        {
            entry.Course = await _db.GetByIdAsync<Course>(entry.CourseId);
            Entries.Add(entry);
        }
        IsBusy = false;
    }

    partial void OnSelectedDayChanged(StudyDay value)
    {
        _ = LoadTimetableAsync();
    }

    [RelayCommand]
    private async Task AddEntryAsync()
    {
        await Shell.Current.GoToAsync("TimetableEntryPage");
    }

    [RelayCommand]
    private async Task EditEntryAsync(TimetableEntry entry)
    {
        if (entry is null) return;
        var navParams = new Dictionary<string, object> { { "TimetableEntry", entry } };
        await Shell.Current.GoToAsync("TimetableEntryPage", navParams);
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(TimetableEntry entry)
    {
        if (entry is null) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Timetable Entry",
            "Are you sure you want to delete this lecture?", "Yes", "No");

        if (confirm)
        {
            await _db.DeleteAsync(entry);
            Entries.Remove(entry);
        }
    }
}
