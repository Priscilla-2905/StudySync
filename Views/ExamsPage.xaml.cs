using StudySync.ViewModels;

namespace StudySync.Views;

public partial class ExamsPage : ContentPage
{
    private readonly ExamsViewModel _viewModel;

    public ExamsPage(ExamsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadExamsAsync();
    }
}
