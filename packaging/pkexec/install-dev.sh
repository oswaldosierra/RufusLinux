#!/usr/bin/env bash
#
# Dev-only installer: publishes RufusLinux.Helper as a self-contained binary
# (runtime embedded, no dependency on DOTNET_ROOT/PATH), installs it to
# /usr/local/bin, and installs the polkit policy so that
# `pkexec /usr/local/bin/rufuslinux-helper <job.json>` shows a native graphical
# password prompt instead of failing or falling back to a console prompt.
#
# Run as your normal user (NOT sudo) — it will call sudo itself only for the
# install steps that actually need root. Re-run after any change to
# RufusLinux.Helper source.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

if [[ $EUID -eq 0 ]]; then
  echo "Run this script as your normal user, not with sudo (it will prompt for sudo itself when needed)." >&2
  exit 1
fi

echo ">> Publishing RufusLinux.Helper (self-contained, linux-x64)..."
dotnet publish "$REPO_ROOT/src/RufusLinux.Helper/RufusLinux.Helper.csproj" \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o "$REPO_ROOT/artifacts/helper"

echo ">> Installing helper binary to /usr/local/bin/rufuslinux-helper (sudo)..."
sudo install -m 0755 -o root -g root "$REPO_ROOT/artifacts/helper/RufusLinux.Helper" /usr/local/bin/rufuslinux-helper

echo ">> Installing polkit policy (sudo)..."
sudo install -m 0644 -o root -g root "$REPO_ROOT/packaging/polkit/org.rufuslinux.helper.policy" \
  /usr/share/polkit-1/actions/org.rufuslinux.helper.policy

echo ""
echo "Done. You can now run:"
echo "  pkexec /usr/local/bin/rufuslinux-helper <job.json>"
echo "and a graphical authentication prompt should appear."
