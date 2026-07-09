using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace RufusLinux.UI.Services;

public sealed class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Window _owner;

    public AvaloniaFilePickerService(Window owner)
    {
        _owner = owner;
    }

    public async Task<string?> PickIsoFileAsync()
    {
        var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Please select a disk image",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Disc images (*.iso)") { Patterns = new[] { "*.iso" } },
                FilePickerFileTypes.All,
            }
        });

        return files.FirstOrDefault()?.TryGetLocalPath();
    }
}
