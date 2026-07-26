using System.Collections.ObservableServices;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class ExamsViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    public ObservableCollection<Exam> Exams { get; } = new();

    public ExamsViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Exams Tracker";
    }

    public async Task LoadExamsAsync()
    {
        IsBusy = true;
        Exams.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetExamsAsync(userId);
        var courses = (await _db.GetCoursesAsync(userId)).ToDictionary(c => c.Id, c => c);

        foreach (var exam in list.OrderBy(e => e.ExamDate))
        {
            exam.Course = courses.TryGetValue(exam.CourseId, out var c) ? c : null;
            Exams.Add(exam);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task AddExamAsync()
    {
        await Shell.Current.GoToAsync("ExamDetailPage");
    }

    [RelayCommand]
    private async Task EditExamAsync(Exam exam)
    {
        if (exam is null) return;
        var navParams = new Dictionary<string, object> { { "Exam", exam } };
        await Shell.Current.GoToAsync("ExamDetailPage", navParams);
    }

    [RelayCommand]
    private async Task DeleteExamAsync(Exam exam)
    {
        if (exam is null) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Exam",
            "Are you sure you want to delete this exam?", "Yes", "No");

        if (confirm)
        {
            await _db.DeleteAsync(exam);
            Exams.Remove(exam);
        }
    }
}
