using RufusLinux.Core.Jobs;

namespace RufusLinux.Helper.Operations;

public static class Formatter
{
    private const int SectorSizeBytes = 512;

    public static async Task FormatAsync(string partitionPath, FileSystemType fileSystem, string volumeLabel, int? clusterSizeBytes, bool quickFormat)
    {
        switch (fileSystem)
        {
            case FileSystemType.Fat32:
                await FormatFat32Async(partitionPath, volumeLabel, clusterSizeBytes);
                break;
            case FileSystemType.Ntfs:
                await FormatNtfsAsync(partitionPath, volumeLabel, clusterSizeBytes, quickFormat);
                break;
            case FileSystemType.ExFat:
                await FormatExFatAsync(partitionPath, volumeLabel, clusterSizeBytes);
                break;
            default:
                throw new NotSupportedException($"Unsupported filesystem: {fileSystem}");
        }
    }

    private static async Task FormatFat32Async(string partitionPath, string volumeLabel, int? clusterSizeBytes)
    {
        var args = new List<string> { "-F", "32", "-n", volumeLabel };
        if (clusterSizeBytes is { } bytes)
        {
            args.Add("-s");
            args.Add((bytes / SectorSizeBytes).ToString());
        }
        args.Add(partitionPath);

        await ProcessRunner.RunAsync("mkfs.vfat", args.ToArray());
    }

    private static async Task FormatNtfsAsync(string partitionPath, string volumeLabel, int? clusterSizeBytes, bool quickFormat)
    {
        var args = new List<string>();
        if (quickFormat)
        {
            args.Add("-Q");
        }
        args.Add("-L");
        args.Add(volumeLabel);
        if (clusterSizeBytes is { } bytes)
        {
            args.Add("-c");
            args.Add(bytes.ToString());
        }
        args.Add(partitionPath);

        await ProcessRunner.RunAsync("mkfs.ntfs", args.ToArray());
    }

    private static async Task FormatExFatAsync(string partitionPath, string volumeLabel, int? clusterSizeBytes)
    {
        var args = new List<string> { "-n", volumeLabel };
        if (clusterSizeBytes is { } bytes)
        {
            args.Add("-c");
            args.Add(bytes.ToString());
        }
        args.Add(partitionPath);

        await ProcessRunner.RunAsync("mkfs.exfat", args.ToArray());
    }
}
