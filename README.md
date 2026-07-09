<p align="center">
  <img src="packaging/icons/hicolor/256x256/apps/rufuslinux.png" alt="RufusLinux logo" width="160">
</p>

<h1 align="center">RufusLinux</h1>

<p align="center">
  A graphical tool for creating bootable USB drives from ISO images on Linux,
  mirroring the layout and workflow of the Windows tool <a href="https://rufus.ie">Rufus</a>.
</p>

## Features

- MBR/GPT partitioning, BIOS/UEFI-CSM/UEFI (non CSM) target systems
- FAT32/NTFS/exFAT filesystem support with cluster size selection
- Windows ISO detection, including ISOHybrid / oversized `install.wim`/`install.esd` handling
- Live device enumeration (USB hotplug via udev)
- Real-time progress and log output while writing
- Automatic UI language: English / Spanish (follows your system locale)

## Screenshots

_Add a screenshot of the app here._

## Requirements

- Linux (x86_64)
- `parted`, `dosfstools`, `ntfs-3g`, `exfatprogs`, `rsync`, `util-linux`, `policykit-1`

These are installed automatically as dependencies of the `.deb` package.

## Installation

Download the latest `.deb` from the [Releases](../../releases) page, then:

```bash
sudo apt install ./rufuslinux_<version>_amd64.deb
```

## Building from source

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (net10.0).

```bash
git clone <this-repo-url>
cd RufusLinux
bash packaging/deb/build.sh
```

The resulting package is written to `artifacts/deb/rufuslinux_<version>_amd64.deb`.

### Project layout

| Project | Purpose |
|---|---|
| `src/RufusLinux.UI` | Avalonia desktop UI (the app users run) |
| `src/RufusLinux.Helper` | Privileged helper invoked via polkit to perform disk operations |
| `src/RufusLinux.Core` | Shared device/ISO/job model used by both UI and Helper |
| `src/RufusLinux.Tests` | Unit tests |
| `packaging/` | `.deb` build script, desktop entry, polkit policy |

### Running tests

```bash
dotnet test src/RufusLinux.Tests/RufusLinux.Tests.csproj
```

## How it works

The UI runs unprivileged. When a write job starts, it hands a job spec to
`RufusLinux.Helper`, launched with elevated privileges through a
[polkit](https://www.freedesktop.org/software/polkit/docs/latest/) policy
(`packaging/polkit/org.rufuslinux.helper.policy`). The helper performs the
actual unmount/partition/format/copy steps and streams structured progress
back to the UI over stdout.

## Contributing

Issues and pull requests are welcome. Please open an issue describing the bug
or feature before submitting a large PR.

## License

[GPL-3.0](LICENSE)
