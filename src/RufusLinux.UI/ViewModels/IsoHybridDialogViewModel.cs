using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RufusLinux.UI.Localization;

namespace RufusLinux.UI.ViewModels;

public partial class IsoHybridDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _message = Strings.IsoHybridMessage;

    public event Action<bool?>? CloseRequested;

    [RelayCommand]
    private void UseNtfs() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void KeepFat32() => CloseRequested?.Invoke(false);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(null);
}
