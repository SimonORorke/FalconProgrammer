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

echo "==> Ensuring Rust target architectures are installed..."
rustup target add aarch64-apple-darwin
rustup target add x86_64-apple-darwin

echo "==> Building release binaries for Apple Silicon (arm64) and Intel (x86_64)..."
cargo build --release --target aarch64-apple-darwin
cargo build --release --target x86_64-apple-darwin

echo "==> Creating macOS app bundle structure..."
cargo bundle --release --target aarch64-apple-darwin

APP_BUNDLE_PATH="target/aarch64-apple-darwin/release/bundle/osx/PitchGrid-Continuum Bridge.app"
APP_BINARY_PATH="$APP_BUNDLE_PATH/Contents/MacOS/pitchgrid_continuum"

echo "==> Merging binaries into a Universal Mach-O binary using lipo..."
lipo -create \
  "target/aarch64-apple-darwin/release/pitchgrid_continuum" \
  "target/x86_64-apple-darwin/release/pitchgrid_continuum" \
  -output "$APP_BINARY_PATH"

echo "===> Automating Ad-Hoc Signing"
codesign --force --deep -s - "$APP_BUNDLE_PATH"

echo "==> Verifying binary architecture..."
file "$APP_BINARY_PATH"

# Extract crate version from Cargo metadata
VERSION=$(cargo metadata --format-version 1 --no-deps | sed -n 's/.*"version":"\([^"]*\)".*/\1/p')

DMG_OUTPUT_DIR="installer/macos"
DMG_OUTPUT_PATH="$DMG_OUTPUT_DIR/PitchGrid-Continuum-Bridge-${VERSION}-Universal.dmg"

echo "==> Packaging into DMG with create-dmg..."
"$CREATE_DMG_CMD" \
  --volname "PitchGrid-Continuum Bridge" \
  --background "$SCRIPT_DIR/dmg_background.png" \
  --window-pos 200 120 \
  --window-size 600 400 \
  --icon-size 100 \
  --icon "PitchGrid-Continuum Bridge.app" 160 190 \
  --hide-extension "PitchGrid-Continuum Bridge.app" \
  --app-drop-link 440 190 \
  --overwrite \
  "$DMG_OUTPUT_PATH" \
  "target/aarch64-apple-darwin/release/bundle/osx"

echo "==> Done! Universal DMG generated at: $DMG_OUTPUT_PATH"