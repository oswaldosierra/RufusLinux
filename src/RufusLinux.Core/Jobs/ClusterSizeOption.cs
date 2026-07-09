namespace RufusLinux.Core.Jobs;

public sealed record ClusterSizeOption(string Label, int? Bytes)
{
    public static ClusterSizeOption Default { get; } = new("default", null);

    public override string ToString() => Label;

    public static IReadOnlyList<ClusterSizeOption> ForFileSystem(FileSystemType fs) => fs switch
    {
        FileSystemType.Fat32 => new[]
        {
            Default,
            new ClusterSizeOption("4096 bytes", 4096),
            new ClusterSizeOption("8192 bytes", 8192),
            new ClusterSizeOption("16384 bytes", 16384),
            new ClusterSizeOption("32768 bytes", 32768),
            new ClusterSizeOption("65536 bytes", 65536),
        },
        FileSystemType.Ntfs => new[]
        {
            Default,
            new ClusterSizeOption("512 bytes", 512),
            new ClusterSizeOption("1024 bytes", 1024),
            new ClusterSizeOption("2048 bytes", 2048),
            new ClusterSizeOption("4096 bytes", 4096),
            new ClusterSizeOption("8192 bytes", 8192),
            new ClusterSizeOption("16384 bytes", 16384),
            new ClusterSizeOption("32768 bytes", 32768),
            new ClusterSizeOption("65536 bytes", 65536),
        },
        FileSystemType.ExFat => new[]
        {
            Default,
            new ClusterSizeOption("4096 bytes", 4096),
            new ClusterSizeOption("32768 bytes", 32768),
            new ClusterSizeOption("131072 bytes", 131072),
            new ClusterSizeOption("1048576 bytes (1 MB)", 1_048_576),
            new ClusterSizeOption("33554432 bytes (32 MB)", 33_554_432),
        },
        _ => new[] { Default }
    };
}
