using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        var (success, message) = await _authService.LoginAsync(Email, Password, RememberMe);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            ErrorMessage = message;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }
}
