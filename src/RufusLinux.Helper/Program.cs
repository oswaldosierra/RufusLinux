using RufusLinux.Core.Jobs;
using RufusLinux.Helper.Operations;
using RufusLinux.Helper.Reporting;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: rufuslinux-helper <job-spec.json>");
    return 1;
}

string jobFilePath = args[0];

WriteJobSpec spec;
try
{
    string json = await File.ReadAllTextAsync(jobFilePath);
    spec = WriteJobSerializer.Deserialize(json);
}
catch (Exception ex)
{
    StdoutProtocol.Error("load-job", $"Failed to read job spec: {ex.Message}");
    return 1;
}

try
{
    StdoutProtocol.Running("guard");
    await DeviceGuard.ValidateAsync(spec.DevicePath, spec.AllowLoopDevices);
    StdoutProtocol.Done("guard");

    StdoutProtocol.Running("unmount");
    await Unmounter.UnmountAllAsync(spec.DevicePath);
    StdoutProtocol.Done("unmount");

    StdoutProtocol.Running("partition");
    string partitionPath = await Partitioner.CreatePartitionAsync(
        spec.DevicePath, spec.PartitionScheme, spec.TargetSystem, spec.FileSystem);
    StdoutProtocol.Done("partition");

    if (spec.WriteMode == WriteMode.IsoCopy)
    {
        StdoutProtocol.Running("format");
        await Formatter.FormatAsync(partitionPath, spec.FileSystem, spec.VolumeLabel, spec.ClusterSizeBytes, spec.QuickFormat);
        StdoutProtocol.Done("format");

        StdoutProtocol.Running("copy");
        await IsoCopier.CopyAsync(spec.IsoPath, partitionPath);
        StdoutProtocol.Done("copy");
    }
    else
    {
        StdoutProtocol.Running("copy");
        await DdWriter.WriteAsync(spec.IsoPath, spec.DevicePath);
        StdoutProtocol.Done("copy");
    }

    StdoutProtocol.Done("all");
    return 0;
}
catch (DeviceGuard.DeviceGuardException ex)
{
    StdoutProtocol.Error("guard", ex.Message);
    return 1;
}
catch (ProcessRunner.CommandFailedException ex)
{
    StdoutProtocol.Error("write", ex.Message);
    return 1;
}
catch (Exception ex)
{
    StdoutProtocol.Error("write", $"Unexpected error: {ex.Message}");
    return 1;
}
finally
{
    try
    {
        File.Delete(jobFilePath);
    }
    catch
    {
        // best-effort cleanup
    }
}
