using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RufusLinux.Core.Jobs;
using RufusLinux.Core.Progress;

namespace RufusLinux.UI.Services;

public sealed class WriteOrchestratorService
{
    private const string InstalledHelperPath = "/usr/local/bin/rufuslinux-helper";

    public async Task<int> RunAsync(WriteJobSpec spec, Action<ProgressEvent> onProgress, Action<string> onRawLine)
    {
        string helperPath = ResolveHelperPath();
        string jobFilePath = Path.Combine(Path.GetTempPath(), $"rufuslinux-job-{Guid.NewGuid():N}.json");

        await File.WriteAllTextAsync(jobFilePath, WriteJobSerializer.Serialize(spec));
        try
        {
            File.SetUnixFileMode(jobFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch
        {
            // best-effort: not all filesystems support POSIX permission bits
        }

        var psi = new ProcessStartInfo("pkexec")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(helperPath);
        psi.ArgumentList.Add(jobFilePath);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start pkexec.");

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                return;
            }
            onRawLine(args.Data);
            var evt = ProgressLineParser.TryParse(args.Data);
            if (evt is not null)
            {
                onProgress(evt);
            }
        };
        process.BeginOutputReadLine();

        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode is 126 or 127)
        {
            onProgress(new ProgressEvent("auth", "error", Message: "Authentication cancelled or not authorized."));
        }
        else if (!string.IsNullOrWhiteSpace(stderr))
        {
            onRawLine(stderr.Trim());
        }

        return process.ExitCode;
    }

    private static string ResolveHelperPath()
    {
        if (File.Exists(InstalledHelperPath))
        {
            return InstalledHelperPath;
        }

        string baseDir = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "RufusLinux.Helper", "bin", "Debug", "net10.0", "RufusLinux.Helper")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "RufusLinux.Helper", "bin", "Release", "net10.0", "RufusLinux.Helper")),
        };

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            "Could not locate rufuslinux-helper. Install it to /usr/local/bin (see packaging/pkexec/install-dev.sh) or build it in dev mode.");
    }
}
