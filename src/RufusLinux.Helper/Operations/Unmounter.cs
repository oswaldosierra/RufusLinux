using System.Diagnostics;

namespace RufusLinux.Helper.Operations;

public static class Unmounter
{
    public static async Task UnmountAllAsync(string devicePath)
    {
        await ProcessRunner.RunBestEffortAsync("umount", devicePath);

        foreach (string partition in await ListPartitionsAsync(devicePath))
        {
            await ProcessRunner.RunBestEffortAsync("umount", partition);
            await ProcessRunner.RunBestEffortAsync("udisksctl", "unmount", "-b", partition);
        }
    }

    private static async Task<IReadOnlyList<string>> ListPartitionsAsync(string devicePath)
    {
        var psi = new ProcessStartInfo("lsblk")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-nlo");
        psi.ArgumentList.Add("PATH,TYPE");
        psi.ArgumentList.Add(devicePath);

        using var process = Process.Start(psi);
        if (process is null)
        {
            return Array.Empty<string>();
        }

        string stdout = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var partitions = new List<string>();
        foreach (string line in stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[1] == "part")
            {
                partitions.Add(parts[0]);
            }
        }
        return partitions;
    }
}
