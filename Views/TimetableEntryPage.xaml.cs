using StudySync.ViewModels;

namespace StudySync.Views;

public partial class TimetableEntryPage : ContentPage
{
    private readonly TimetableEntryViewModel _viewModel;

    public TimetableEntryPage(TimetableEntryViewModel viewModel)
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
