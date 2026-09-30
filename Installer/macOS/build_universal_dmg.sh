#!/usr/bin/env bash
set -euo pipefail
# Running the script generates two 'deprecated' warnings for `hdiutil`.
# Cost-Benefit Analysis of the Deprecation Warnings
# In evaluating whether to eliminate these warnings, we should weigh the tangible benefits
# (cleaner log output, forward-proofing) against the actual costs and risks
# (breakage of build environments, developer time, and toolchain constraints).
# On balance, leaving them as-is for now yields the highest net utility and stability.

# Navigate to the repository root directory (one level up from installer/macos)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$PROJECT_ROOT"

echo "==> Ensuring create-dmg is available..."
CREATE_DMG_CMD=""
if command -v create-dmg >/dev/null 2>&1; then
  CREATE_DMG_CMD="create-dmg"
elif [ -x "$PROJECT_ROOT/target/tools/create-dmg/create-dmg" ]; then
  CREATE_DMG_CMD="$PROJECT_ROOT/target/tools/create-dmg/create-dmg"
else
  echo "==> Downloading create-dmg..."
  mkdir -p "$PROJECT_ROOT/target/tools"
  git clone --depth 1 https://github.com/create-dmg/create-dmg.git "$PROJECT_ROOT/target/tools/create-dmg"
  CREATE_DMG_CMD="$PROJECT_ROOT/target/tools/create-dmg/create-dmg"
fi

echo "==> Building release binaries for Apple Silicon (osx-arm64) and Intel (osx-x64)..."
dotnet publish FalconProgrammer/FalconProgrammer.csproj -c Release -r osx-arm64 --self-contained false -o "$PROJECT_ROOT/bin/publish/osx-arm64"
dotnet publish FalconProgrammer/FalconProgrammer.csproj -c Release -r osx-x64 --self-contained false -o "$PROJECT_ROOT/bin/publish/osx-x64"

echo "==> Creating macOS app bundle structure..."
APP_BUNDLE_PATH="$PROJECT_ROOT/bin/bundle/osx/Falcon Programmer.app"
rm -rf "$APP_BUNDLE_PATH"
mkdir -p "$APP_BUNDLE_PATH/Contents/MacOS"
mkdir -p "$APP_BUNDLE_PATH/Contents/Resources"

# Copy published files
cp -R "$PROJECT_ROOT/bin/publish/osx-arm64/"* "$APP_BUNDLE_PATH/Contents/MacOS/"

# Merge the native app hosts using lipo into a Universal Mach-O binary
lipo -create \
  "$PROJECT_ROOT/bin/publish/osx-arm64/FalconProgrammer" \
  "$PROJECT_ROOT/bin/publish/osx-x64/FalconProgrammer" \
  -output "$APP_BUNDLE_PATH/Contents/MacOS/FalconProgrammer"
chmod +x "$APP_BUNDLE_PATH/Contents/MacOS/FalconProgrammer"

# Copy the squircle icon asset to Resources
cp "$PROJECT_ROOT/FalconProgrammer/Assets/falcon_svg_repo_com_512_512.icns" "$APP_BUNDLE_PATH/Contents/Resources/falcon_svg_repo_com_512_512.icns"

# Extract version from csproj
VERSION=$(grep -o '<Version>[^<]*</Version>' FalconProgrammer/FalconProgrammer.csproj | head -n 1 | sed -e 's/<[^>]*>//g')

cat << PLIST > "$APP_BUNDLE_PATH/Contents/Info.plist"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>FalconProgrammer</string>
    <key>CFBundleIconFile</key>
    <string>falcon_svg_repo_com_512_512.icns</string>
    <key>CFBundleIdentifier</key>
    <string>com.simonororke.FalconProgrammer</string>
    <key>CFBundleName</key>
    <string>Falcon Programmer</string>
    <key>CFBundleDisplayName</key>
    <string>Falcon Programmer</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
PLIST

echo "===> Automating Ad-Hoc Signing"
codesign --force --deep -s - "$APP_BUNDLE_PATH"

echo "==> Verifying binary architecture..."
file "$APP_BUNDLE_PATH/Contents/MacOS/FalconProgrammer"

DMG_OUTPUT_DIR="Installer/macOS"
DMG_OUTPUT_PATH="$DMG_OUTPUT_DIR/FalconProgrammer-${VERSION}-Universal.dmg"

# Ensure background image exists
if [ ! -f "$SCRIPT_DIR/dmg_background.png" ]; then
  echo "==> Generating DMG background image..."
  swift "$SCRIPT_DIR/generate_dmg_background.swift" "$SCRIPT_DIR/dmg_background.png"
fi

echo "==> Packaging into DMG with create-dmg..."
"$CREATE_DMG_CMD" \
  --volname "Falcon Programmer" \
  --background "$SCRIPT_DIR/dmg_background.png" \
  --window-pos 200 120 \
  --window-size 600 400 \
  --icon-size 100 \
  --icon "Falcon Programmer.app" 160 190 \
  --hide-extension "Falcon Programmer.app" \
  --app-drop-link 440 190 \
  --overwrite \
  "$DMG_OUTPUT_PATH" \
  "$PROJECT_ROOT/bin/bundle/osx"

echo "==> Done! Universal DMG generated at: $DMG_OUTPUT_PATH"