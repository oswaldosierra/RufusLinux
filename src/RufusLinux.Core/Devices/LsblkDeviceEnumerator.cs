using System.Diagnostics;
using System.Text.Json;

namespace RufusLinux.Core.Devices;

public sealed class LsblkDeviceEnumerator
{
    public async Task<IReadOnlyList<UsbDevice>> EnumerateAsync(CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo("lsblk")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-J");
        psi.ArgumentList.Add("-b");
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add("NAME,PATH,TRAN,MODEL,SIZE,TYPE,RM,HOTPLUG,MOUNTPOINT");

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start lsblk.");

        string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(stdout))
        {
            return Array.Empty<UsbDevice>();
        }

        return Parse(stdout);
    }

    public static IReadOnlyList<UsbDevice> Parse(string lsblkJson)
    {
        var result = new List<UsbDevice>();

        using var doc = JsonDocument.Parse(lsblkJson);
        if (!doc.RootElement.TryGetProperty("blockdevices", out var devices))
        {
            return result;
        }

        foreach (var device in devices.EnumerateArray())
        {
            string type = device.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
            if (type != "disk")
            {
                continue;
            }

            string? tran = device.TryGetProperty("tran", out var trEl) && trEl.ValueKind == JsonValueKind.String
                ? trEl.GetString()
                : null;
            bool removable = device.TryGetProperty("rm", out var rmEl) && rmEl.ValueKind == JsonValueKind.True;
            bool hotplug = device.TryGetProperty("hotplug", out var hpEl) && hpEl.ValueKind == JsonValueKind.True;

            bool isUsb = tran == "usb";
            bool looksRemovable = removable && hotplug;
            if (!isUsb && !looksRemovable)
            {
                continue;
            }

            string path = device.GetProperty("path").GetString() ?? "";
            string? model = device.TryGetProperty("model", out var mEl) && mEl.ValueKind == JsonValueKind.String
                ? mEl.GetString()?.Trim()
                : null;
            long size = device.TryGetProperty("size", out var sEl) && sEl.ValueKind == JsonValueKind.Number
                ? sEl.GetInt64()
                : 0;

            result.Add(new UsbDevice(path, model, size, tran ?? "unknown"));
        }

        return result;
    }
}
