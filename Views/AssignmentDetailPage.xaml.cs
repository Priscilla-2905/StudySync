using StudySync.ViewModels;

namespace StudySync.Views;

public partial class AssignmentDetailPage : ContentPage
{
    private readonly AssignmentDetailViewModel _viewModel;

    public AssignmentDetailPage(AssignmentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
