using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RufusLinux.Core.Jobs;
using RufusLinux.UI.Localization;

namespace RufusLinux.UI.Converters;

public sealed class EnumDisplayConverter : IValueConverter
{
    public static readonly EnumDisplayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        PartitionScheme.Gpt => "GPT",
        PartitionScheme.Mbr => "MBR",
        TargetSystem.BiosOrUefiCsm => Strings.BiosOrUefiCsm,
        TargetSystem.UefiNonCsm => Strings.UefiNonCsm,
        FileSystemType.Fat32 => "FAT32",
        FileSystemType.Ntfs => "NTFS",
        FileSystemType.ExFat => "exFAT",
        _ => value?.ToString() ?? string.Empty
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
