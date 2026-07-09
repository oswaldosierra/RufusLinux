using System.Diagnostics;

namespace RufusLinux.Helper.Operations;

public static class ProcessRunner
{
    public sealed class CommandFailedException : Exception
    {
        public CommandFailedException(string command, int exitCode, string stderr)
            : base($"'{command}' exited with code {exitCode}: {stderr}")
        {
        }
    }

    public static async Task RunAsync(string fileName, params string[] args)
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
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new CommandFailedException($"{fileName} {string.Join(' ', args)}", process.ExitCode, stderr);
        }
    }

    public static async Task RunBestEffortAsync(string fileName, params string[] args)
    {
        try
        {
            await RunAsync(fileName, args);
        }
        catch
        {
            // best-effort: caller does not care if this particular command fails
        }
    }
}
