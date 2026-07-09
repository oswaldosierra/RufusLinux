using System.Threading.Tasks;

namespace RufusLinux.UI.Services;

public interface IDialogService
{
    /// <returns>true = switch to NTFS, false = keep FAT32, null = cancel ISO selection.</returns>
    Task<bool?> ShowIsoHybridDialogAsync();

    Task<bool> ShowConfirmAsync(string title, string message);
}
