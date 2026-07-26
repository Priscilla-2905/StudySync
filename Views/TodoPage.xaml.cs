using StudySync.ViewModels;

namespace StudySync.Views;

public partial class TodoPage : ContentPage
{
    private readonly TodoViewModel _viewModel;

    public TodoPage(TodoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTodosAsync();
    }
}
