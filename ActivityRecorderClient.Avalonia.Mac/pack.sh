#!/bin/bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

ARG="${1:-}"

if [[ -z "$ARG" ]]; then
  HOST_ARCH="$(uname -m)"
  if [[ "$HOST_ARCH" == "arm64" ]]; then
    PLATFORM="osx-arm64"
  else
    PLATFORM="osx-x64"
  fi
else
  if [[ "$ARG" == "x64" || "$ARG" == "osx-x64" ]]; then
    PLATFORM="osx-x64"
  elif [[ "$ARG" == "arm64" || "$ARG" == "osx-arm64" || "$ARG" == "arm" ]]; then
    PLATFORM="osx-arm64"
  else
    echo "Usage: $0 [x64|arm64]"
    exit 1
  fi
fi

PUBLISH_DIR="$PROJECT_ROOT/bin/Release/net8.0-macos/$PLATFORM/publish"
APP_DIR="$PROJECT_ROOT/bin/Release/net8.0-macos/$PLATFORM/JC360.app"

PKGROOT="$PROJECT_ROOT/bin/Release/net8.0-macos/$PLATFORM/pkgroot"
SCRIPTS_DIR="$PROJECT_ROOT/bin/Release/net8.0-macos/$PLATFORM/pkgscripts"
LA_PLIST_SRC="$PROJECT_ROOT/com.jc360.autostart.plist"
LA_PLIST_DST="$PKGROOT/Library/LaunchAgents/com.jc360.autostart.plist"
ENTITLEMENTS="$PROJECT_ROOT/../entitlements.plist"

IDENTIFIER="com.JobCTRL.JobCTRL"

DEVID_APP="Developer ID Application: JobCTRL Informatikai Korlatolt Felelossegu Tarsasag (68PZUUGBU9)"
DEVID_INSTALLER="Developer ID Installer: JobCTRL Informatikai Korlatolt Felelossegu Tarsasag (68PZUUGBU9)"

get_csproj_version() {
  /usr/bin/python3 - "$1" <<'PY'
import sys, xml.etree.ElementTree as ET

path = sys.argv[1]
t = ET.parse(path)
r = t.getroot()

def text(tag):
  el = r.find(f".//{tag}")
  return (el.text or "").strip() if el is not None else ""

v = text("Version")
if v:
  print(v); raise SystemExit(0)

vp = text("VersionPrefix")
vs = text("VersionSuffix")
if vp and vs:
  print(f"{vp}-{vs}"); raise SystemExit(0)
if vp:
  print(vp); raise SystemExit(0)

# last resort
print("1.0")
PY
}

APP_VERSION="${APP_VERSION_OVERRIDE:-$(get_csproj_version "$PROJECT_ROOT/ActivityRecorderClient.Avalonia.Mac.csproj")}"
echo "Using package version: $APP_VERSION"

dotnet publish "$PROJECT_ROOT/ActivityRecorderClient.Avalonia.Mac.csproj" \
  -c Release -r "$PLATFORM" --self-contained true /p:DefineConstants=WINDOWS%3BAppConfigDefault

# Staging payload felépítése a PKG-hez:
rm -rf "$PKGROOT" "$SCRIPTS_DIR"
mkdir -p "$PKGROOT/Applications" "$PKGROOT/Library/LaunchAgents" "$SCRIPTS_DIR"

# App a /Applications alá (staging példány lesz a "forrás" innentől)
rsync -a --delete "$APP_DIR" "$PKGROOT/Applications/"
find "$PKGROOT" -name '._*' -delete || true
find "$PKGROOT" -name '.DS_Store' -delete || true

STAGED_APP="$PKGROOT/Applications/JC360.app"

# Bundle-root szemét takarítása (gyakori ok az "unsealed contents..." hibára)
# Csak a Contents maradjon a .app gyökerében.
find "$STAGED_APP" -maxdepth 1 -mindepth 1 \
  ! -name "Contents" \
  -exec rm -rf {} + || true

# LaunchAgent a /Library/LaunchAgents alá
cp "$LA_PLIST_SRC" "$LA_PLIST_DST"
chmod 644 "$LA_PLIST_DST"

# Native lib patch/sign lépések a STAGED appon (ne az eredetin)
echo "Signing dylibs..."
find "$STAGED_APP/Contents/MonoBundle" -name "*.dylib" -exec \
    codesign --force --options runtime --timestamp --sign "$DEVID_APP" {} \;

echo "Signing other binaries..."
find "$STAGED_APP/Contents/MonoBundle" -type f -perm +111 ! -name "*.dylib" -exec \
    codesign --force --options runtime --timestamp --sign "$DEVID_APP" {} \;
echo "Signing app bundle..."
codesign --force --options runtime --timestamp --entitlements "$ENTITLEMENTS" --sign "$DEVID_APP" --deep "$STAGED_APP"

echo "Verifying..."
codesign --verify --deep --strict --verbose=2 "$STAGED_APP" || exit 1

# PKG scripts ((post|pre)install)
cp "$PROJECT_ROOT/scripts"/* "$SCRIPTS_DIR/"
chmod +x "$SCRIPTS_DIR"/*

mkdir -p "$PUBLISH_DIR"

COMP_PLIST="$PROJECT_ROOT/bin/Release/net8.0-macos/$PLATFORM/component.plist"

# 1) komponens plist generálása a payloadból
pkgbuild --analyze --root "$PKGROOT" "$COMP_PLIST"

# 2) Bundle relocation kikapcsolása a JC360.app-ra
/usr/bin/python3 - "$COMP_PLIST" <<'PY'
import sys, plistlib

path = sys.argv[1]
with open(path, "rb") as f:
    arr = plistlib.load(f)

target = "Applications/JC360.app"
hit = False
for d in arr:
    # pkgbuild analyze tipikusan RootRelativeBundlePath-ot ad bundle-ökhöz
    if d.get("RootRelativeBundlePath") == target or d.get("BundlePath") == target:
        d["BundleIsRelocatable"] = False
        d["BundleHasStrictIdentifier"] = True
        hit = True

if not hit:
    print(f"WARNING: {target} not found in component plist; relocation may still occur.", file=sys.stderr)

with open(path, "wb") as f:
    plistlib.dump(arr, f)
PY

pkgbuild \
  --root "$PKGROOT" \
  --component-plist "$COMP_PLIST" \
  --scripts "$SCRIPTS_DIR" \
  --identifier "$IDENTIFIER" \
  --version "$APP_VERSION" \
  --install-location "/" \
  --sign "$DEVID_INSTALLER" \
  "$PUBLISH_DIR/JC360-$PLATFORM.pkg"

productsign --sign "$DEVID_INSTALLER" "$PUBLISH_DIR/JC360-$PLATFORM.pkg" "$PUBLISH_DIR/JC360-$PLATFORM-signed.pkg"

xcrun notarytool submit "$PUBLISH_DIR/JC360-$PLATFORM-signed.pkg" --keychain-profile "JC360_NOTARY" --wait
xcrun stapler staple "$PUBLISH_DIR/JC360-$PLATFORM-signed.pkg"
