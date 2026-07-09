using System.Globalization;
using System.Resources;

namespace RufusLinux.UI.Localization;

public static class Strings
{
    private static readonly ResourceManager Manager =
        new("RufusLinux.UI.Resources.Strings", typeof(Strings).Assembly);

    private static string Get(string key) => Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string DeviceHeader => Get(nameof(DeviceHeader));
    public static string RefreshDeviceListTooltip => Get(nameof(RefreshDeviceListTooltip));
    public static string BootSelectionHeader => Get(nameof(BootSelectionHeader));
    public static string SelectButton => Get(nameof(SelectButton));
    public static string PartitionSchemeLabel => Get(nameof(PartitionSchemeLabel));
    public static string TargetSystemLabel => Get(nameof(TargetSystemLabel));
    public static string FormatOptionsHeader => Get(nameof(FormatOptionsHeader));
    public static string VolumeLabelLabel => Get(nameof(VolumeLabelLabel));
    public static string FileSystemLabel => Get(nameof(FileSystemLabel));
    public static string ClusterSizeLabel => Get(nameof(ClusterSizeLabel));
    public static string QuickFormatLabel => Get(nameof(QuickFormatLabel));
    public static string ExtendedLabelLabel => Get(nameof(ExtendedLabelLabel));
    public static string ShowHideLogTooltip => Get(nameof(ShowHideLogTooltip));
    public static string StartButton => Get(nameof(StartButton));
    public static string CloseButton => Get(nameof(CloseButton));
    public static string OkButton => Get(nameof(OkButton));
    public static string CancelButton => Get(nameof(CancelButton));
    public static string UseNtfsButton => Get(nameof(UseNtfsButton));
    public static string KeepFat32Button => Get(nameof(KeepFat32Button));
    public static string IsoHybridTitle => Get(nameof(IsoHybridTitle));
    public static string IsoHybridMessage => Get(nameof(IsoHybridMessage));
    public static string ConfirmTitle => Get(nameof(ConfirmTitle));
    public static string AreYouSure => Get(nameof(AreYouSure));
    public static string ConfirmDestroyMessage => Get(nameof(ConfirmDestroyMessage));
    public static string NoBootSelected => Get(nameof(NoBootSelected));
    public static string StandardWindowsInstallation => Get(nameof(StandardWindowsInstallation));
    public static string WindowsToGo => Get(nameof(WindowsToGo));
    public static string Ready => Get(nameof(Ready));
    public static string AnalyzingIso => Get(nameof(AnalyzingIso));
    public static string NoDeviceSelected => Get(nameof(NoDeviceSelected));
    public static string NoImageSelected => Get(nameof(NoImageSelected));
    public static string Starting => Get(nameof(Starting));
    public static string Done => Get(nameof(Done));
    public static string Error => Get(nameof(Error));
    public static string ErrorWithMessage => Get(nameof(ErrorWithMessage));
    public static string CheckingDevice => Get(nameof(CheckingDevice));
    public static string Unmounting => Get(nameof(Unmounting));
    public static string Partitioning => Get(nameof(Partitioning));
    public static string Formatting => Get(nameof(Formatting));
    public static string CopyingFiles => Get(nameof(CopyingFiles));
    public static string BiosOrUefiCsm => Get(nameof(BiosOrUefiCsm));
    public static string UefiNonCsm => Get(nameof(UefiNonCsm));
}
