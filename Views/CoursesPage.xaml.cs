using StudySync.ViewModels;

namespace StudySync.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _viewModel;

    public CoursesPage(CoursesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCoursesAsync();
    }
}
