using System.Diagnostics;

namespace RufusLinux.Core.Iso;

public sealed class IsoInspector
{
    public async Task<WindowsIsoMetadata> InspectAsync(string isoPath, CancellationToken cancellationToken = default)
    {
        var entries = await ListEntriesAsync(isoPath, cancellationToken);
        return Classify(entries);
    }

    public static WindowsIsoMetadata Classify(IReadOnlyList<(string Path, long Size)> entries)
    {
        bool hasBootWim = false;
        bool hasInstallWim = false;
        bool hasInstallEsd = false;
        long largestSourceFile = 0;

        foreach (var (path, size) in entries)
        {
            string normalized = path.Replace('\\', '/').ToLowerInvariant();

            if (normalized == "sources/boot.wim")
            {
                hasBootWim = true;
            }
            if (normalized == "sources/install.wim")
            {
                hasInstallWim = true;
            }
            if (normalized == "sources/install.esd")
            {
                hasInstallEsd = true;
            }
            if (normalized.StartsWith("sources/") && size > largestSourceFile)
            {
                largestSourceFile = size;
            }
        }

        bool isWindowsIso = hasBootWim && (hasInstallWim || hasInstallEsd);
        return new WindowsIsoMetadata(isWindowsIso, hasInstallWim, hasInstallEsd, largestSourceFile);
    }

    private static async Task<IReadOnlyList<(string Path, long Size)>> ListEntriesAsync(
        string isoPath, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo("7z")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("l");
        psi.ArgumentList.Add("-slt");
        psi.ArgumentList.Add(isoPath);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start 7z.");

        string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            return Array.Empty<(string, long)>();
        }

        return ParseSltOutput(stdout);
    }

    public static IReadOnlyList<(string Path, long Size)> ParseSltOutput(string sltOutput)
    {
        var result = new List<(string, long)>();
        string? currentPath = null;
        long currentSize = -1;

        foreach (string rawLine in sltOutput.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');

            if (line.Length == 0)
            {
                if (currentPath is not null && currentSize >= 0)
                {
                    result.Add((currentPath, currentSize));
                }
                currentPath = null;
                currentSize = -1;
                continue;
            }

            if (line.StartsWith("Path = "))
            {
                currentPath = line["Path = ".Length..];
            }
            else if (line.StartsWith("Size = "))
            {
                if (long.TryParse(line["Size = ".Length..], out long size))
                {
                    currentSize = size;
                }
            }
        }

        if (currentPath is not null && currentSize >= 0)
        {
            result.Add((currentPath, currentSize));
        }

        return result;
    }
}
