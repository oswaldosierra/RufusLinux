using RufusLinux.Core.Jobs;

namespace RufusLinux.Helper.Operations;

public static class Partitioner
{
    public static async Task<string> CreatePartitionAsync(string devicePath, PartitionScheme scheme, TargetSystem targetSystem, FileSystemType fileSystem)
    {
        await ProcessRunner.RunAsync("wipefs", "-a", devicePath);

        string label = scheme == PartitionScheme.Gpt ? "gpt" : "msdos";
        await ProcessRunner.RunAsync("parted", "-s", devicePath, "mklabel", label);
        await ProcessRunner.RunAsync("parted", "-s", devicePath, "mkpart", "primary", "1MiB", "100%");

        string? flag = DetermineFlag(scheme, targetSystem, fileSystem);
        if (flag is not null)
        {
            await ProcessRunner.RunAsync("parted", "-s", devicePath, "set", "1", flag, "on");
        }

        await ProcessRunner.RunBestEffortAsync("partprobe", devicePath);
        await Task.Delay(1500);

        return PartitionPathFor(devicePath);
    }

    private static string? DetermineFlag(PartitionScheme scheme, TargetSystem targetSystem, FileSystemType fileSystem)
    {
        if (scheme == PartitionScheme.Gpt)
        {
            if (targetSystem == TargetSystem.UefiNonCsm && (fileSystem == FileSystemType.Fat32))
            {
                return "esp";
            }
            return "msftdata";
        }

        return "boot";
    }

    public static string PartitionPathFor(string devicePath)
    {
        bool endsWithDigit = devicePath.Length > 0 && char.IsDigit(devicePath[^1]);
        return endsWithDigit ? $"{devicePath}p1" : $"{devicePath}1";
    }
}
