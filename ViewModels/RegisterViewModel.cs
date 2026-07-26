using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudySync.Services.Interfaces;

namespace StudySync.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Create Account";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        var (success, message) = await _authService.RegisterAsync(FullName, Email, Password, ConfirmPassword);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.DisplayAlert("Success", "Account created successfully! Please sign in.", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            ErrorMessage = message;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
