#!/usr/bin/env bash
#
# Builds a rufuslinux_<version>_amd64.deb package: self-contained UI and
# helper binaries (no dependency on system/user .NET install), the polkit
# policy, and a desktop menu entry.
#
# Run as your normal user — dpkg-deb --root-owner-group lets us stamp
# root:root ownership in the .deb without actually needing root to build it.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
VERSION="0.1.1"
PKG_NAME="rufuslinux_${VERSION}_amd64"
BUILD_DIR="$REPO_ROOT/artifacts/deb/$PKG_NAME"

echo ">> Cleaning previous build..."
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR/DEBIAN" \
         "$BUILD_DIR/opt/rufuslinux" \
         "$BUILD_DIR/usr/local/bin" \
         "$BUILD_DIR/usr/bin" \
         "$BUILD_DIR/usr/share/applications" \
         "$BUILD_DIR/usr/share/polkit-1/actions" \
         "$BUILD_DIR/usr/share/icons/hicolor"

echo ">> Publishing RufusLinux.UI (self-contained, linux-x64)..."
dotnet publish "$REPO_ROOT/src/RufusLinux.UI/RufusLinux.UI.csproj" \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$REPO_ROOT/artifacts/publish/ui"

echo ">> Publishing RufusLinux.Helper (self-contained, linux-x64)..."
dotnet publish "$REPO_ROOT/src/RufusLinux.Helper/RufusLinux.Helper.csproj" \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o "$REPO_ROOT/artifacts/publish/helper"

echo ">> Assembling package tree..."
cp "$REPO_ROOT/artifacts/publish/ui/RufusLinux.UI" "$BUILD_DIR/opt/rufuslinux/RufusLinux.UI"
chmod 0755 "$BUILD_DIR/opt/rufuslinux/RufusLinux.UI"
ln -s /opt/rufuslinux/RufusLinux.UI "$BUILD_DIR/usr/bin/rufuslinux"

cp "$REPO_ROOT/artifacts/publish/helper/RufusLinux.Helper" "$BUILD_DIR/usr/local/bin/rufuslinux-helper"
chmod 0755 "$BUILD_DIR/usr/local/bin/rufuslinux-helper"

cp "$SCRIPT_DIR/rufuslinux.desktop" "$BUILD_DIR/usr/share/applications/rufuslinux.desktop"
cp "$REPO_ROOT/packaging/polkit/org.rufuslinux.helper.policy" \
   "$BUILD_DIR/usr/share/polkit-1/actions/org.rufuslinux.helper.policy"

echo ">> Installing hicolor icons..."
cp -r "$REPO_ROOT/packaging/icons/hicolor/." "$BUILD_DIR/usr/share/icons/hicolor/"

cp "$SCRIPT_DIR/control" "$BUILD_DIR/DEBIAN/control"
cp "$SCRIPT_DIR/postinst" "$BUILD_DIR/DEBIAN/postinst"
chmod 0755 "$BUILD_DIR/DEBIAN/postinst"

echo ">> Building .deb..."
dpkg-deb --root-owner-group --build "$BUILD_DIR" "$REPO_ROOT/artifacts/deb/${PKG_NAME}.deb"

echo ""
echo "Done: $REPO_ROOT/artifacts/deb/${PKG_NAME}.deb"
echo "Install with:  sudo apt install $REPO_ROOT/artifacts/deb/${PKG_NAME}.deb"
