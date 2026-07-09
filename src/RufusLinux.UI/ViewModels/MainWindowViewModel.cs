using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RufusLinux.Core.Devices;
using RufusLinux.Core.Iso;
using RufusLinux.Core.Jobs;
using RufusLinux.Core.Progress;
using RufusLinux.UI.Localization;
using RufusLinux.UI.Services;

namespace RufusLinux.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private static string NoBootSelected => Strings.NoBootSelected;

    private readonly DeviceEnumerationService _deviceService;
    private readonly IsoInspectionService _isoInspectionService;
    private readonly IFilePickerService? _filePickerService;
    private readonly IDialogService? _dialogService;
    private readonly WriteOrchestratorService _writeOrchestrator = new();

    public ObservableCollection<UsbDevice> Devices { get; } = new();

    [ObservableProperty]
    private UsbDevice? _selectedDevice;

    [ObservableProperty]
    private string? _selectedIsoPath;

    public MainWindowViewModel() : this(new DeviceEnumerationService(), new IsoInspectionService(), null, null)
    {
    }

    public MainWindowViewModel(
        DeviceEnumerationService deviceService,
        IsoInspectionService isoInspectionService,
        IFilePickerService? filePickerService,
        IDialogService? dialogService)
    {
        _deviceService = deviceService;
        _isoInspectionService = isoInspectionService;
        _filePickerService = filePickerService;
        _dialogService = dialogService;
        _deviceService.DevicesChanged += OnDevicesChanged;
        _deviceService.StartWatching();
        _ = RefreshDevicesAsync();
    }

    private void OnDevicesChanged()
    {
        Dispatcher.UIThread.Post(() => _ = RefreshDevicesAsync());
    }

    private async Task RefreshDevicesAsync()
    {
        string? previouslySelectedPath = SelectedDevice?.Path;
        var devices = await _deviceService.EnumerateAsync();

        Devices.Clear();
        foreach (var device in devices)
        {
            Devices.Add(device);
        }

        SelectedDevice = previouslySelectedPath is not null
            ? Devices.FirstOrDefault(d => d.Path == previouslySelectedPath) ?? Devices.FirstOrDefault()
            : Devices.FirstOrDefault();
    }

    public void Dispose()
    {
        _deviceService.DevicesChanged -= OnDevicesChanged;
        _deviceService.Dispose();
    }

    public ObservableCollection<string> BootSources { get; } = new()
    {
        Strings.NoBootSelected,
    };

    [ObservableProperty]
    private string? _selectedBootSource = Strings.NoBootSelected;

    public ObservableCollection<string> ImageOptions { get; } = new()
    {
        Strings.StandardWindowsInstallation,
        Strings.WindowsToGo,
    };

    [ObservableProperty]
    private string? _selectedImageOption = Strings.StandardWindowsInstallation;

    [ObservableProperty]
    private bool _isWindowsIsoLoaded;

    private WindowsIsoMetadata? _currentIsoMetadata;

    public ObservableCollection<PartitionScheme> PartitionSchemes { get; } = new()
    {
        PartitionScheme.Gpt,
        PartitionScheme.Mbr,
    };

    [ObservableProperty]
    private PartitionScheme _selectedPartitionScheme = PartitionScheme.Gpt;

    public ObservableCollection<TargetSystem> TargetSystems { get; } =
        new(TargetSystemsFor(PartitionScheme.Gpt));

    [ObservableProperty]
    private TargetSystem _selectedTargetSystem = TargetSystem.UefiNonCsm;

    private static IReadOnlyList<TargetSystem> TargetSystemsFor(PartitionScheme scheme) => scheme switch
    {
        // Rufus locks GPT to UEFI (non CSM) only; MBR allows either BIOS/UEFI-CSM or UEFI (non CSM).
        PartitionScheme.Gpt => new[] { TargetSystem.UefiNonCsm },
        _ => new[] { TargetSystem.BiosOrUefiCsm, TargetSystem.UefiNonCsm },
    };

    [ObservableProperty]
    private string _volumeLabel = "WIN11";

    public ObservableCollection<FileSystemType> FileSystems { get; } = new()
    {
        FileSystemType.Ntfs,
        FileSystemType.Fat32,
        FileSystemType.ExFat,
    };

    [ObservableProperty]
    private FileSystemType _selectedFileSystem = FileSystemType.Ntfs;

    public ObservableCollection<ClusterSizeOption> ClusterSizes { get; private set; } =
        new(ClusterSizeOption.ForFileSystem(FileSystemType.Ntfs));

    [ObservableProperty]
    private ClusterSizeOption _selectedClusterSize = ClusterSizeOption.Default;

    [ObservableProperty]
    private bool _quickFormat = true;

    [ObservableProperty]
    private bool _extendedLabel = true;

    [ObservableProperty]
    private string _statusText = Strings.Ready;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private bool _isLogVisible;

    [ObservableProperty]
    private string _logText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public string LogToggleGlyph => IsLogVisible ? "▲" : "▼";

    partial void OnIsLogVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(LogToggleGlyph));
    }

    partial void OnSelectedFileSystemChanged(FileSystemType value)
    {
        ClusterSizes = new ObservableCollection<ClusterSizeOption>(ClusterSizeOption.ForFileSystem(value));
        OnPropertyChanged(nameof(ClusterSizes));
        SelectedClusterSize = ClusterSizeOption.Default;

        if (value == FileSystemType.Fat32 && _currentIsoMetadata is { HasFileExceedingFat32Limit: true })
        {
            _ = ResolveFat32OversizedConflictAsync();
        }
    }

    partial void OnSelectedPartitionSchemeChanged(PartitionScheme value)
    {
        var allowed = TargetSystemsFor(value);

        TargetSystems.Clear();
        foreach (var system in allowed)
        {
            TargetSystems.Add(system);
        }

        if (!allowed.Contains(SelectedTargetSystem))
        {
            SelectedTargetSystem = value == PartitionScheme.Gpt
                ? TargetSystem.UefiNonCsm
                : TargetSystem.BiosOrUefiCsm;
        }
    }

    [RelayCommand]
    private async Task RefreshDevices()
    {
        await RefreshDevicesAsync();
    }

    [RelayCommand]
    private async Task SelectIso()
    {
        if (_filePickerService is null)
        {
            return;
        }

        string? path = await _filePickerService.PickIsoFileAsync();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        SelectedIsoPath = path;

        string fileName = Path.GetFileName(path);
        BootSources.Clear();
        BootSources.Add(fileName);
        SelectedBootSource = fileName;

        StatusText = Strings.AnalyzingIso;
        var metadata = await _isoInspectionService.InspectAsync(path);
        StatusText = Strings.Ready;

        _currentIsoMetadata = metadata;
        IsWindowsIsoLoaded = metadata.IsWindowsIso;

        if (metadata.IsWindowsIso)
        {
            VolumeLabel = Path.GetFileNameWithoutExtension(path).ToUpperInvariant();
            if (VolumeLabel.Length > 32)
            {
                VolumeLabel = VolumeLabel[..32];
            }

            if (metadata.HasFileExceedingFat32Limit && SelectedFileSystem == FileSystemType.Fat32)
            {
                bool? proceedResult = await ResolveFat32OversizedConflictAsync();
                if (proceedResult is null)
                {
                    // Cancelled: undo the ISO selection entirely.
                    SelectedIsoPath = null;
                    _currentIsoMetadata = null;
                    IsWindowsIsoLoaded = false;
                    BootSources.Clear();
                    BootSources.Add(NoBootSelected);
                    SelectedBootSource = NoBootSelected;
                }
            }
        }
    }

    private async Task<bool?> ResolveFat32OversizedConflictAsync()
    {
        if (_dialogService is null)
        {
            // No dialog surface available (e.g. design-time): default to the safe choice.
            SelectedFileSystem = FileSystemType.Ntfs;
            return true;
        }

        bool? result = await _dialogService.ShowIsoHybridDialogAsync();
        if (result == true)
        {
            SelectedFileSystem = FileSystemType.Ntfs;
        }
        return result;
    }


    [RelayCommand]
    private void ToggleLog()
    {
        IsLogVisible = !IsLogVisible;
    }

    private bool CanStart() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        if (SelectedDevice is null)
        {
            StatusText = Strings.NoDeviceSelected;
            return;
        }
        if (string.IsNullOrEmpty(SelectedIsoPath))
        {
            StatusText = Strings.NoImageSelected;
            return;
        }

        bool confirmed = _dialogService is null
            || await _dialogService.ShowConfirmAsync(
                "RufusLinux",
                string.Format(Strings.ConfirmDestroyMessage, SelectedDevice.DisplayName));
        if (!confirmed)
        {
            return;
        }

        var spec = new WriteJobSpec(
            SelectedDevice.Path,
            SelectedIsoPath,
            SelectedPartitionScheme,
            SelectedTargetSystem,
            SelectedFileSystem,
            SelectedClusterSize.Bytes,
            VolumeLabel,
            QuickFormat,
            WriteMode.IsoCopy);

        IsBusy = true;
        StartCommand.NotifyCanExecuteChanged();
        ProgressPercent = 0;
        LogText = string.Empty;
        StatusText = Strings.Starting;

        try
        {
            int exitCode = await _writeOrchestrator.RunAsync(
                spec,
                onProgress: evt => Dispatcher.UIThread.Post(() => ApplyProgress(evt)),
                onRawLine: line => Dispatcher.UIThread.Post(() => AppendLog(line)));

            StatusText = exitCode == 0 ? Strings.Done : Strings.Error;
        }
        catch (Exception ex)
        {
            StatusText = Strings.Error;
            AppendLog(ex.Message);
        }
        finally
        {
            IsBusy = false;
            StartCommand.NotifyCanExecuteChanged();
        }
    }

    private void ApplyProgress(ProgressEvent evt)
    {
        if (evt.Percent is { } percent)
        {
            ProgressPercent = percent;
        }

        StatusText = (evt.Stage, evt.Status) switch
        {
            ("guard", "running") => Strings.CheckingDevice,
            ("unmount", "running") => Strings.Unmounting,
            ("partition", "running") => Strings.Partitioning,
            ("format", "running") => Strings.Formatting,
            ("copy", "running") => Strings.CopyingFiles,
            ("all", "done") => Strings.Done,
            (_, "error") => string.Format(Strings.ErrorWithMessage, evt.Message),
            _ => StatusText,
        };
    }

    private void AppendLog(string line)
    {
        LogText += line + Environment.NewLine;
    }

    [RelayCommand]
    private void Close()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
        }
    }
}
