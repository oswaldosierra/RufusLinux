using RufusLinux.Core.Iso;

namespace RufusLinux.Tests;

public class IsoInspectorTests
{
    private const string SampleSltOutput = """
    Path = sources
    Folder = +
    Size = 0

    Path = sources/boot.wim
    Folder = -
    Size = 452510208

    Path = sources/install.wim
    Folder = -
    Size = 7557825550

    Path = readme.txt
    Folder = -
    Size = 128

    """;

    [Fact]
    public void ParseSltOutput_ExtractsPathsAndSizes()
    {
        var entries = IsoInspector.ParseSltOutput(SampleSltOutput);

        Assert.Contains(entries, e => e.Path == "sources/install.wim" && e.Size == 7557825550);
        Assert.Contains(entries, e => e.Path == "sources/boot.wim" && e.Size == 452510208);
    }

    [Fact]
    public void Classify_DetectsWindowsIsoWithOversizedInstallWim()
    {
        var entries = IsoInspector.ParseSltOutput(SampleSltOutput);
        var metadata = IsoInspector.Classify(entries);

        Assert.True(metadata.IsWindowsIso);
        Assert.True(metadata.HasInstallWim);
        Assert.False(metadata.HasInstallEsd);
        Assert.True(metadata.HasFileExceedingFat32Limit);
        Assert.Equal(7557825550, metadata.LargestSourceFileBytes);
    }

    [Fact]
    public void Classify_NonWindowsIso_ReturnsNotWindows()
    {
        var entries = new[] { ("some/linux/file.squashfs", 100_000_000L) };
        var metadata = IsoInspector.Classify(entries);

        Assert.False(metadata.IsWindowsIso);
    }
}
