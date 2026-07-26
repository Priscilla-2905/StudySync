using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Helpers;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

[QueryProperty(nameof(TimetableEntry), "TimetableEntry")]
public partial class TimetableEntryViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private TimetableEntry? _timetableEntry;
    [ObservableProperty] private Course? _selectedCourse;
    [ObservableProperty] private StudyDay _selectedDay = StudyDay.Monday;
    [ObservableProperty] private TimeSpan _startTime = TimeSpan.FromHours(9);
    [ObservableProperty] private TimeSpan _endTime = TimeSpan.FromHours(11);
    [ObservableProperty] private string _venue = string.Empty;
    [ObservableProperty] private string _lecturer = string.Empty;

    public ObservableCollection<Course> Courses { get; } = new();
    public List<StudyDay> DaysOfWeek { get; } = Enum.GetValues<StudyDay>().ToList();

    public TimetableEntryViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Add Lecture";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        Courses.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetCoursesAsync(userId);
        foreach (var c in list) Courses.Add(c);

        if (TimetableEntry is not null)
        {
            SelectedCourse = Courses.FirstOrDefault(c => c.Id == TimetableEntry.CourseId);
        }
        else if (Courses.Count > 0)
        {
            SelectedCourse = Courses[0];
        }

        IsBusy = false;
    }

    partial void OnTimetableEntryChanged(TimetableEntry? value)
    {
        if (value is not null)
        {
            Title = "Edit Lecture";
            SelectedDay = value.Day;
            StartTime = value.StartTime;
            EndTime = value.EndTime;
            Venue = value.Venue;
            Lecturer = value.Lecturer;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedCourse is null)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please select a course.", "OK");
            return;
        }

        if (EndTime <= StartTime)
        {
            await Shell.Current.DisplayAlert("Validation Error", "End time must be after start time.", "OK");
            return;
        }

        var userId = _authService.CurrentUserId;

        // Check for overlapping lectures
        var existingEntries = await _db.GetTimetableForDayAsync(userId, SelectedDay);
        var overlap = existingEntries.FirstOrDefault(e =>
            (TimetableEntry is null || e.Id != TimetableEntry.Id) &&
            ValidationHelper.DoTimesOverlap(StartTime, EndTime, e.StartTime, e.EndTime));

        if (overlap is not null)
        {
            await Shell.Current.DisplayAlert("Time Conflict",
                "This lecture overlaps with an existing lecture on the same day.", "OK");
            return;
        }

        var item = TimetableEntry ?? new TimetableEntry { UserId = userId };
        item.CourseId = SelectedCourse.Id;
        item.Day = SelectedDay;
        item.StartTime = StartTime;
        item.EndTime = EndTime;
        item.Venue = Venue.Trim();
        item.Lecturer = Lecturer.Trim();

        await _db.SaveAsync(item);
        await Shell.Current.GoToAsync("..");
    }
}
