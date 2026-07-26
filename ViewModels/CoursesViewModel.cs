using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class CoursesViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    public ObservableCollection<Course> Courses { get; } = new();

    public CoursesViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "My Courses";
    }

    public async Task LoadCoursesAsync()
    {
        IsBusy = true;
        Courses.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetCoursesAsync(userId);
        foreach (var c in list) Courses.Add(c);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task AddCourseAsync()
    {
        await Shell.Current.GoToAsync("CourseDetailPage");
    }

    [RelayCommand]
    private async Task EditCourseAsync(Course course)
    {
        if (course is null) return;
        var navParams = new Dictionary<string, object> { { "Course", course } };
        await Shell.Current.GoToAsync("CourseDetailPage", navParams);
    }

    [RelayCommand]
    private async Task DeleteCourseAsync(Course course)
    {
        if (course is null) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete Course",
            $"Are you sure you want to delete {course.CourseCode}?", "Yes", "No");

        if (confirm)
        {
            await _db.DeleteAsync(course);
            Courses.Remove(course);
        }
    }
}
