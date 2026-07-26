using StudySync.ViewModels;

namespace StudySync.Views;

public partial class AssignmentsPage : ContentPage
{
    private readonly AssignmentsViewModel _viewModel;

    public AssignmentsPage(AssignmentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAssignmentsAsync();
    }
}
