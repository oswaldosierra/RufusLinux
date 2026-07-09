namespace RufusLinux.Core.Jobs;

public sealed record WriteJobSpec(
    string DevicePath,
    string IsoPath,
    PartitionScheme PartitionScheme,
    TargetSystem TargetSystem,
    FileSystemType FileSystem,
    int? ClusterSizeBytes,
    string VolumeLabel,
    bool QuickFormat,
    WriteMode WriteMode,
    bool AllowLoopDevices = false);
