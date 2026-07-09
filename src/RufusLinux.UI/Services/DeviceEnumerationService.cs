using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RufusLinux.Core.Devices;

namespace RufusLinux.UI.Services;

public sealed class DeviceEnumerationService : IDisposable
{
    private readonly LsblkDeviceEnumerator _enumerator = new();
    private readonly UdevDeviceWatcher _watcher = new();

    public event Action? DevicesChanged
    {
        add => _watcher.DevicesChanged += value;
        remove => _watcher.DevicesChanged -= value;
    }

    public void StartWatching() => _watcher.Start();

    public Task<IReadOnlyList<UsbDevice>> EnumerateAsync(CancellationToken cancellationToken = default)
        => _enumerator.EnumerateAsync(cancellationToken);

    public void Dispose() => _watcher.Dispose();
}
