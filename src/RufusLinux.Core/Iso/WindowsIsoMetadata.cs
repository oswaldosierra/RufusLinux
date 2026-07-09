namespace RufusLinux.Core.Iso;

public sealed record WindowsIsoMetadata(
    bool IsWindowsIso,
    bool HasInstallWim,
    bool HasInstallEsd,
    long LargestSourceFileBytes)
{
    public const long FourGiB = 4L * 1024 * 1024 * 1024;

    public bool HasFileExceedingFat32Limit => LargestSourceFileBytes > FourGiB;

    public static WindowsIsoMetadata NotWindows { get; } = new(false, false, false, 0);
}
