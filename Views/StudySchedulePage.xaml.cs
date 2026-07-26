using StudySync.ViewModels;

namespace StudySync.Views;

public partial class StudySchedulePage : ContentPage
{
    private readonly StudyScheduleViewModel _viewModel;

    public StudySchedulePage(StudyScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSessionsAsync();
    }
}
