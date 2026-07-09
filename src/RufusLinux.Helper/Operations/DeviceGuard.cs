using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RufusLinux.Helper.Operations;

public static class DeviceGuard
{
    private static readonly Regex AllowedDevicePattern = new(
        @"^/dev/(sd[a-z]+|nvme\d+n\d+|mmcblk\d+)$",
        RegexOptions.Compiled);

    private static readonly Regex AllowedLoopDevicePattern = new(
        @"^/dev/loop\d+$",
        RegexOptions.Compiled);

    public sealed class DeviceGuardException : Exception
    {
        public DeviceGuardException(string message) : base(message)
        {
        }
    }

    public static async Task ValidateAsync(string devicePath, bool allowLoopDevices)
    {
        bool matchesAllowList = AllowedDevicePattern.IsMatch(devicePath)
            || (allowLoopDevices && AllowedLoopDevicePattern.IsMatch(devicePath));

        if (!matchesAllowList)
        {
            throw new DeviceGuardException(
                $"Refusing to operate on '{devicePath}': does not match an allowed whole-disk device pattern.");
        }

        string? rootSource = await GetMountSourceAsync("/");
        if (rootSource is not null && IsSameOrParentDevice(devicePath, rootSource))
        {
            throw new DeviceGuardException(
                $"Refusing to operate on '{devicePath}': it hosts the running system's root filesystem.");
        }

        if (!allowLoopDevices)
        {
            bool isRemovableUsb = await IsRemovableUsbAsync(devicePath);
            if (!isRemovableUsb)
            {
                throw new DeviceGuardException(
                    $"Refusing to operate on '{devicePath}': not a removable USB device according to lsblk.");
            }
        }
    }

    private static bool IsSameOrParentDevice(string devicePath, string mountSource)
    {
        return mountSource.StartsWith(devicePath, StringComparison.Ordinal);
    }

    private static async Task<string?> GetMountSourceAsync(string mountpoint)
    {
        var (exitCode, stdout, _) = await RunAsync("findmnt", "-n", "-o", "SOURCE", mountpoint);
        return exitCode == 0 ? stdout.Trim() : null;
    }

    private static async Task<bool> IsRemovableUsbAsync(string devicePath)
    {
        var (exitCode, stdout, _) = await RunAsync("lsblk", "-ndo", "TRAN,RM,HOTPLUG", devicePath);
        if (exitCode != 0)
        {
            return false;
        }

        string[] parts = stdout.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string tran = parts.Length > 0 ? parts[0] : "";
        bool rm = parts.Length > 1 && parts[1] == "1";
        bool hotplug = parts.Length > 2 && parts[2] == "1";

        return tran == "usb" || (rm && hotplug);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var a in args)
        {
            psi.ArgumentList.Add(a);
        }

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }
}
