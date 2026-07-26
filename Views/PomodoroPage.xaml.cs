using StudySync.ViewModels;

namespace StudySync.Views;

public partial class PomodoroPage : ContentPage
{
    private readonly PomodoroViewModel _viewModel;

    public PomodoroPage(PomodoroViewModel viewModel)
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
