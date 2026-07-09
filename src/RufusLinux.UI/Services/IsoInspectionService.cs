using System.Threading;
using System.Threading.Tasks;
using RufusLinux.Core.Iso;

namespace RufusLinux.UI.Services;

public sealed class IsoInspectionService
{
    private readonly IsoInspector _inspector = new();

    public Task<WindowsIsoMetadata> InspectAsync(string isoPath, CancellationToken cancellationToken = default)
        => _inspector.InspectAsync(isoPath, cancellationToken);
}
