using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RufusLinux.UI.Localization;

namespace RufusLinux.UI.ViewModels;

public partial class ConfirmDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = Strings.ConfirmTitle;

    [ObservableProperty]
    private string _message = Strings.AreYouSure;

    public event Action<bool>? CloseRequested;

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
