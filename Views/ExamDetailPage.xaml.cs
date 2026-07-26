using StudySync.ViewModels;

namespace StudySync.Views;

public partial class ExamDetailPage : ContentPage
{
    private readonly ExamDetailViewModel _viewModel;

    public ExamDetailPage(ExamDetailViewModel viewModel)
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
