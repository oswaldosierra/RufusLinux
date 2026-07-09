using System.Threading.Tasks;
using Avalonia.Controls;
using RufusLinux.UI.ViewModels;
using RufusLinux.UI.Views;

namespace RufusLinux.UI.Services;

public sealed class AvaloniaDialogService : IDialogService
{
    private readonly Window _owner;

    public AvaloniaDialogService(Window owner)
    {
        _owner = owner;
    }

    public Task<bool?> ShowIsoHybridDialogAsync()
    {
        var tcs = new TaskCompletionSource<bool?>();
        var viewModel = new IsoHybridDialogViewModel();
        var dialog = new IsoHybridDialog { DataContext = viewModel };

        viewModel.CloseRequested += result =>
        {
            tcs.TrySetResult(result);
            dialog.Close();
        };

        dialog.ShowDialog(_owner);
        return tcs.Task;
    }

    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var viewModel = new ConfirmDialogViewModel { Title = title, Message = message };
        var dialog = new ConfirmDialog { DataContext = viewModel };

        viewModel.CloseRequested += result =>
        {
            tcs.TrySetResult(result);
            dialog.Close();
        };

        dialog.ShowDialog(_owner);
        return tcs.Task;
    }
}
