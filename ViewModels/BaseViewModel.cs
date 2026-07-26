using CommunityToolkit.Mvvm.ComponentModel;

namespace StudySync.ViewModels;

/// <summary>
/// Abstract base class for all ViewModels in StudySync.
/// Inherits from ObservableObject to provide INotifyPropertyChanged.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    public bool IsNotBusy => !IsBusy;
}
