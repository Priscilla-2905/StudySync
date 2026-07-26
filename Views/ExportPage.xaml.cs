using StudySync.ViewModels;

namespace StudySync.Views;

public partial class ExportPage : ContentPage
{
    public ExportPage(ExportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
