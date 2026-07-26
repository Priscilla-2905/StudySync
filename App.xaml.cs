using StudySync.Services.Interfaces;

namespace StudySync;

public partial class App : Application
{
    public App(IAuthService authService)
    {
        InitializeComponent();

        MainPage = new AppShell();

        // Check for session auto-login
        Task.Run(async () =>
        {
            var loggedIn = await authService.TryAutoLoginAsync();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (loggedIn)
                {
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
            });
        });
    }
}
