using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using RufusLinux.Helper.Reporting;

namespace RufusLinux.Helper.Operations;

public static class DdWriter
{
    private static readonly Regex BytesCopiedPattern = new(@"^(\d+)\s+bytes", RegexOptions.Compiled);

    public static async Task WriteAsync(string isoPath, string devicePath)
    {
        long totalBytes = new FileInfo(isoPath).Length;

        var psi = new ProcessStartInfo("dd")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add($"if={isoPath}");
        psi.ArgumentList.Add($"of={devicePath}");
        psi.ArgumentList.Add("bs=4M");
        psi.ArgumentList.Add("status=progress");
        psi.ArgumentList.Add("conv=fsync");

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dd.");

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }

            var match = BytesCopiedPattern.Match(args.Data.Trim());
            if (match.Success && long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long bytesCopied))
            {
                double percent = totalBytes > 0 ? Math.Min(100.0, bytesCopied * 100.0 / totalBytes) : 0;
                StdoutProtocol.Running("copy", percent, args.Data.Trim());
            }
        };
        process.BeginErrorReadLine();

        await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new ProcessRunner.CommandFailedException("dd", process.ExitCode, "dd failed, see log above");
        }

        await ProcessRunner.RunAsync("sync");
    }
}
