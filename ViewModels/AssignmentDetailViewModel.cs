using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

[QueryProperty(nameof(Assignment), "Assignment")]
public partial class AssignmentDetailViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;

    [ObservableProperty] private Assignment? _assignment;
    [ObservableProperty] private Course? _selectedCourse;
    [ObservableProperty] private string _assignmentTitle = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private DateTime _deadline = DateTime.Now.AddDays(7);
    [ObservableProperty] private TimeSpan _deadlineTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
    [ObservableProperty] private Priority _priority = Priority.Medium;
    [ObservableProperty] private double _estimatedHours = 2.0;
    [ObservableProperty] private AssignmentStatus _status = AssignmentStatus.Pending;

    public ObservableCollection<Course> Courses { get; } = new();
    public List<Priority> PriorityOptions { get; } = Enum.GetValues<Priority>().ToList();
    public List<AssignmentStatus> StatusOptions { get; } = Enum.GetValues<AssignmentStatus>().ToList();

    public AssignmentDetailViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Add Assignment";
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        Courses.Clear();
        var userId = _authService.CurrentUserId;
        var list = await _db.GetCoursesAsync(userId);
        foreach (var c in list) Courses.Add(c);

        if (Assignment is not null)
        {
            SelectedCourse = Courses.FirstOrDefault(c => c.Id == Assignment.CourseId);
        }
        else if (Courses.Count > 0)
        {
            SelectedCourse = Courses[0];
        }

        IsBusy = false;
    }

    partial void OnAssignmentChanged(Assignment? value)
    {
        if (value is not null)
        {
            Title = "Edit Assignment";
            AssignmentTitle = value.Title;
            Description = value.Description;
            Deadline = value.Deadline.Date;
            DeadlineTime = value.Deadline.TimeOfDay;
            Priority = value.Priority;
            EstimatedHours = value.EstimatedHours;
            Status = value.Status;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(AssignmentTitle))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Assignment title is required.", "OK");
            return;
        }

        if (SelectedCourse is null)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please select a course.", "OK");
            return;
        }

        var userId = _authService.CurrentUserId;
        var fullDeadline = Deadline.Date + DeadlineTime;

        var item = Assignment ?? new Assignment { UserId = userId };
        item.Title = AssignmentTitle.Trim();
        item.CourseId = SelectedCourse.Id;
        item.Description = Description.Trim();
        item.Deadline = fullDeadline;
        item.Priority = Priority;
        item.EstimatedHours = EstimatedHours;
        item.Status = Status;

        await _db.SaveAsync(item);
        await Shell.Current.GoToAsync("..");
    }
}
