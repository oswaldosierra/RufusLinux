using RufusLinux.Core.Devices;

namespace RufusLinux.Tests;

public class LsblkDeviceEnumeratorTests
{
    private const string SampleJson = """
    {
       "blockdevices": [
          {"name":"loop0","path":"/dev/loop0","tran":null,"model":null,"size":4096,"type":"loop","rm":false,"hotplug":false,"mountpoint":"/snap/x/1"},
          {"name":"sda","path":"/dev/sda","tran":"sata","model":"HS-SSD-E100 512G","size":512110190592,"type":"disk","rm":false,"hotplug":false,"mountpoint":null,
           "children":[{"name":"sda1","path":"/dev/sda1","tran":null,"model":null,"size":512110190592,"type":"part","rm":false,"hotplug":false,"mountpoint":"/"}]},
          {"name":"sdb","path":"/dev/sdb","tran":"usb","model":"DataTraveler 3.0","size":61444864000,"type":"disk","rm":true,"hotplug":true,"mountpoint":null,
           "children":[{"name":"sdb1","path":"/dev/sdb1","tran":null,"model":null,"size":61440000000,"type":"part","rm":true,"hotplug":true,"mountpoint":null}]},
          {"name":"mmcblk0","path":"/dev/mmcblk0","tran":null,"model":"SD Card Reader","size":32000000000,"type":"disk","rm":true,"hotplug":true,"mountpoint":null}
       ]
    }
    """;

    [Fact]
    public void Parse_ExcludesNonDiskAndInternalDisks()
    {
        var devices = LsblkDeviceEnumerator.Parse(SampleJson);

        Assert.DoesNotContain(devices, d => d.Path == "/dev/loop0");
        Assert.DoesNotContain(devices, d => d.Path == "/dev/sda");
    }

    [Fact]
    public void Parse_IncludesUsbTransportDisk()
    {
        var devices = LsblkDeviceEnumerator.Parse(SampleJson);

        var usb = Assert.Single(devices, d => d.Path == "/dev/sdb");
        Assert.Equal("DataTraveler 3.0", usb.Model);
        Assert.Equal(61444864000, usb.SizeBytes);
        Assert.Equal("usb", usb.Transport);
    }

    [Fact]
    public void Parse_IncludesRemovableHotplugWithoutUsbTransport()
    {
        var devices = LsblkDeviceEnumerator.Parse(SampleJson);

        Assert.Contains(devices, d => d.Path == "/dev/mmcblk0");
    }

    [Fact]
    public void Parse_EmptyBlockDevices_ReturnsEmpty()
    {
        var devices = LsblkDeviceEnumerator.Parse("""{"blockdevices":[]}""");

        Assert.Empty(devices);
    }
}
