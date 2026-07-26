using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

[QueryProperty(nameof(Course), "Course")]
public partial class CourseDetailViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private Course? _course;
    [ObservableProperty] private string _courseCode = string.Empty;
    [ObservableProperty] private string _courseName = string.Empty;
    [ObservableProperty] private string _lecturer = string.Empty;
    [ObservableProperty] private int _credits = 3;
    [ObservableProperty] private string _selectedColour = "#6C63FF";

    public List<string> ColorOptions { get; } = new()
    {
        "#6C63FF", "#FF6584", "#4CAF50", "#FF9800",
        "#00BCD4", "#9C27B0", "#E91E63", "#3F51B5"
    };

    public CourseDetailViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Add Course";
    }

    partial void OnCourseChanged(Course? value)
    {
        if (value is not null)
        {
            Title = "Edit Course";
            CourseCode = value.CourseCode;
            CourseName = value.CourseName;
            Lecturer = value.Lecturer;
            Credits = value.Credits;
            SelectedColour = value.Colour;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CourseCode) || string.IsNullOrWhiteSpace(CourseName))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Course Code and Name are required.", "OK");
            return;
        }

        var userId = _authService.CurrentUserId;
        var existingCourses = await _db.GetCoursesAsync(userId);

        // Check for duplicate course code
        if (Course is null && existingCourses.Any(c => c.CourseCode.Equals(CourseCode.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            await Shell.Current.DisplayAlert("Duplicate Course", $"Course code '{CourseCode}' already exists.", "OK");
            return;
        }

        var item = Course ?? new Course { UserId = userId };
        item.CourseCode = CourseCode.Trim().ToUpperInvariant();
        item.CourseName = CourseName.Trim();
        item.Lecturer = Lecturer.Trim();
        item.Credits = Credits;
        item.Colour = SelectedColour;

        await _db.SaveAsync(item);
        await Shell.Current.GoToAsync("..");
    }
}
