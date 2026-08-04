using Microsoft.UI.Xaml;

namespace StudySync.WinUI;

public partial class App : MauiWinUIApplication
{
    private static readonly string LogPath = System.IO.Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
        "StudySync_crash.log");

    public App()
    {
        this.UnhandledException += OnUnhandledException;
        try
        {
            this.InitializeComponent();
        }
        catch (System.Exception ex)
        {
            System.IO.File.AppendAllText(LogPath,
                $"[{System.DateTime.Now}] InitializeComponent FAILED:\n{ex}\n\n");
            throw;
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        System.IO.File.AppendAllText(LogPath,
            $"[{System.DateTime.Now}] Unhandled: {e.Exception}\n\n");
    }

    protected override MauiApp CreateMauiApp()
    {
        try
        {
            return StudySync.MauiProgram.CreateMauiApp();
        }
        catch (System.Exception ex)
        {
            System.IO.File.AppendAllText(LogPath,
                $"[{System.DateTime.Now}] CreateMauiApp FAILED:\n{ex}\n\n");
            throw;
        }
    }
}

