using System.Diagnostics;

namespace RufusLinux.Core.Devices;

public sealed class UdevDeviceWatcher : IDisposable
{
    private const int DebounceMilliseconds = 300;

    private Process? _process;
    private Timer? _debounceTimer;
    private readonly object _gate = new();

    public event Action? DevicesChanged;

    public void Start()
    {
        if (_process is not null)
        {
            return;
        }

        var psi = new ProcessStartInfo("udevadm")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("monitor");
        psi.ArgumentList.Add("--udev");
        psi.ArgumentList.Add("--subsystem-match=block");

        try
        {
            _process = Process.Start(psi);
        }
        catch (Exception)
        {
            _process = null;
            return;
        }

        if (_process is null)
        {
            return;
        }

        _process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }
            ScheduleRefresh();
        };
        _process.BeginOutputReadLine();
    }

    private void ScheduleRefresh()
    {
        lock (_gate)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => DevicesChanged?.Invoke(), null, DebounceMilliseconds, Timeout.Infinite);
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
            }
            _process?.Dispose();
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
