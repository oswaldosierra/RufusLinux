using System.Diagnostics;
using System.Text.RegularExpressions;
using RufusLinux.Helper.Reporting;

namespace RufusLinux.Helper.Operations;

public static class IsoCopier
{
    private static readonly Regex ProgressPercentPattern = new(@"(\d+)%", RegexOptions.Compiled);

    public static async Task CopyAsync(string isoPath, string partitionPath)
    {
        string isoMount = Directory.CreateTempSubdirectory("rufuslinux-iso-").FullName;
        string usbMount = Directory.CreateTempSubdirectory("rufuslinux-usb-").FullName;

        try
        {
            await ProcessRunner.RunAsync("mount", "-o", "loop,ro", isoPath, isoMount);
            await ProcessRunner.RunAsync("mount", partitionPath, usbMount);

            await RunRsyncAsync($"{isoMount}/", $"{usbMount}/");

            await ProcessRunner.RunAsync("sync");
        }
        finally
        {
            await ProcessRunner.RunBestEffortAsync("umount", usbMount);
            await ProcessRunner.RunBestEffortAsync("umount", isoMount);
            TryDeleteDirectory(usbMount);
            TryDeleteDirectory(isoMount);
        }
    }

    private static async Task RunRsyncAsync(string source, string destination)
    {
        var psi = new ProcessStartInfo("rsync")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-ah");
        psi.ArgumentList.Add("--info=progress2");
        psi.ArgumentList.Add("--no-perms");
        psi.ArgumentList.Add("--no-owner");
        psi.ArgumentList.Add("--no-group");
        psi.ArgumentList.Add(source);
        psi.ArgumentList.Add(destination);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start rsync.");

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }

            var match = ProgressPercentPattern.Match(args.Data);
            if (match.Success && double.TryParse(match.Groups[1].Value, out double percent))
            {
                StdoutProtocol.Running("copy", percent, args.Data.Trim());
            }
        };
        process.BeginOutputReadLine();

        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new ProcessRunner.CommandFailedException("rsync", process.ExitCode, stderr);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
