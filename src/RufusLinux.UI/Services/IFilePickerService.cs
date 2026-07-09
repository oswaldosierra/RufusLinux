using System.Threading.Tasks;

namespace RufusLinux.UI.Services;

public interface IFilePickerService
{
    Task<string?> PickIsoFileAsync();
}
