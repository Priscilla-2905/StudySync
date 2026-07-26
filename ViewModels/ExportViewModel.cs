using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class ExportViewModel : BaseViewModel
{
    private readonly IExportService _exportService;
    private readonly IAuthService _authService;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public ExportViewModel(IExportService exportService, IAuthService authService)
    {
        _exportService = exportService;
        _authService = authService;
        Title = "Export Data";
    }

    [RelayCommand]
    private async Task ExportJsonAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var userId = _authService.CurrentUserId;
            var path = await _exportService.ExportToJsonAsync(userId);
            StatusMessage = $"JSON exported successfully!";
            await _exportService.ShareFileAsync(path, "StudySync Backup JSON");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var userId = _authService.CurrentUserId;
            var dir = await _exportService.ExportToCsvAsync(userId);
            StatusMessage = $"CSV files saved to: {dir}";
            await Shell.Current.DisplayAlert("CSV Export Complete", $"CSV files saved to folder:\n{dir}", "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }

        IsBusy = false;
    }
}
