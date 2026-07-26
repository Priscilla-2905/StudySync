using StudySync.ViewModels;

namespace StudySync.Views;

public partial class TimetablePage : ContentPage
{
    private readonly TimetableViewModel _viewModel;

    public TimetablePage(TimetableViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTimetableAsync();
    }
}
