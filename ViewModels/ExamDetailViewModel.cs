using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

[QueryProperty(nameof(Exam), "Exam")]
public partial class ExamDetailViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private Exam? _exam;
    [ObservableProperty] private Course? _selectedCourse;
    [ObservableProperty] private DateTime _examDate = DateTime.Now.AddDays(14);
    [ObservableProperty] private TimeSpan _examTime = TimeSpan.FromHours(9);
    [ObservableProperty] private string _venue = string.Empty;
    [ObservableProperty] private Importance _importance = Importance.High;
    [ObservableProperty] private string _notes = string.Empty;

    public ObservableCollection<Course> Courses { get; } = new();
    public List<Importance> ImportanceOptions { get; } = Enum.GetValues<Importance>().ToList();

    public ExamDetailViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Add Exam";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        Courses.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetCoursesAsync(userId);
        foreach (var c in list) Courses.Add(c);

        if (Exam is not null)
        {
            SelectedCourse = Courses.FirstOrDefault(c => c.Id == Exam.CourseId);
        }
        else if (Courses.Count > 0)
        {
            SelectedCourse = Courses[0];
        }

        IsBusy = false;
    }

    partial void OnExamChanged(Exam? value)
    {
        if (value is not null)
        {
            Title = "Edit Exam";
            ExamDate = value.ExamDate.Date;
            ExamTime = value.Time;
            Venue = value.Venue;
            Importance = value.Importance;
            Notes = value.Notes;
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

        var userId = _authService.CurrentUserId;
        var item = Exam ?? new Exam { UserId = userId };
        item.CourseId = SelectedCourse.Id;
        item.ExamDate = ExamDate.Date;
        item.Time = ExamTime;
        item.Venue = Venue.Trim();
        item.Importance = Importance;
        item.Notes = Notes.Trim();

        await _db.SaveAsync(item);
        await Shell.Current.GoToAsync("..");
    }
}
