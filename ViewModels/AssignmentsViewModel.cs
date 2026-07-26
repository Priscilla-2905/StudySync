using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class AssignmentsViewModel : BaseViewModel
{
    private readonly Data.DatabaseService _db;
    private readonly IAuthService _authService;
    private List<Assignment> _allAssignments = new();

    [ObservableProperty] private string _selectedFilter = "All";
    [ObservableProperty] private string _selectedSort = "Deadline";

    public ObservableCollection<Assignment> FilteredAssignments { get; } = new();
    public List<string> FilterOptions { get; } = new() { "All", "Pending", "In Progress", "Completed" };
    public List<string> SortOptions { get; } = new() { "Deadline", "Priority", "Course" };

    public AssignmentsViewModel(Data.DatabaseService db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
        Title = "Assignments";
    }

    public async Task LoadAssignmentsAsync()
    {
        IsBusy = true;
        var userId = _authService.CurrentUserId;
        _allAssignments = await _db.GetAssignmentsAsync(userId);
        var courses = (await _db.GetCoursesAsync(userId)).ToDictionary(c => c.Id, c => c);

        foreach (var a in _allAssignments)
        {
            a.Course = courses.TryGetValue(a.CourseId, out var c) ? c : null;
        }

        ApplyFilterAndSort();
        IsBusy = false;
    }

    partial void OnSelectedFilterChanged(string value) => ApplyFilterAndSort();
    partial void OnSelectedSortChanged(string value) => ApplyFilterAndSort();

    private void ApplyFilterAndSort()
    {
        var items = _allAssignments.AsEnumerable();

        // Filtering
        items = SelectedFilter switch
        {
            "Pending" => items.Where(a => a.Status == AssignmentStatus.Pending),
            "In Progress" => items.Where(a => a.Status == AssignmentStatus.InProgress),
            "Completed" => items.Where(a => a.Status == AssignmentStatus.Completed),
            _ => items
        };

        // Sorting
        items = SelectedSort switch
        {
            "Priority" => items.OrderByDescending(a => a.Priority).ThenBy(a => a.Deadline),
            "Course" => items.OrderBy(a => a.Course?.CourseCode ?? string.Empty).ThenBy(a => a.Deadline),
            _ => items.OrderBy(a => a.Deadline)
        };

        FilteredAssignments.Clear();
        foreach (var item in items) FilteredAssignments.Add(item);
    }

    [RelayCommand]
    private async Task AddAssignmentAsync()
    {
        await Shell.Current.GoToAsync("AssignmentDetailPage");
    }

    [RelayCommand]
    private async Task EditAssignmentAsync(Assignment assignment)
    {
        if (assignment is null) return;
        var navParams = new Dictionary<string, object> { { "Assignment", assignment } };
        await Shell.Current.GoToAsync("AssignmentDetailPage", navParams);
    }

    [RelayCommand]
    private async Task DeleteAssignmentAsync(Assignment assignment)
    {
        if (assignment is null) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Assignment",
            $"Are you sure you want to delete '{assignment.Title}'?", "Yes", "No");

        if (confirm)
        {
            await _db.DeleteAsync(assignment);
            _allAssignments.Remove(assignment);
            FilteredAssignments.Remove(assignment);
        }
    }
}
