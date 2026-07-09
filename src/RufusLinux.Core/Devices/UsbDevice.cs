namespace RufusLinux.Core.Devices;

public sealed record UsbDevice(string Path, string? Model, long SizeBytes, string Transport)
{
    public string DisplayName => $"{(string.IsNullOrWhiteSpace(Model) ? "USB Drive" : Model)} ({FormatSize(SizeBytes)}) [{Path}]";

    private static string FormatSize(long bytes)
    {
        double gb = bytes / 1_000_000_000.0;
        return gb >= 1 ? $"{gb:0.0} GB" : $"{bytes / 1_000_000.0:0.0} MB";
    }
}
