#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_DIR="$ROOT_DIR/sts2-lan-connect"
ASSEMBLY_NAME="sts2_lan_connect"
DOTNET_BIN="${DOTNET_BIN:-$HOME/.dotnet/dotnet}"
GODOT_BIN="${GODOT_BIN:-$HOME/.local/bin/godot451-mono}"
MAC_GAME_DIR="${STS2_ROOT:-$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2}"
MAC_APP_DIR="$MAC_GAME_DIR/SlayTheSpire2.app"
MOD_OUTPUT_DIR="${STS2_MODS_DIR:-$MAC_APP_DIR/Contents/MacOS/mods/$ASSEMBLY_NAME}"
PCK_SOURCE="$PROJECT_DIR/build/$ASSEMBLY_NAME.pck"
DLL_SOURCE="$PROJECT_DIR/.godot/mono/temp/bin/Debug/$ASSEMBLY_NAME.dll"

if [[ ! -x "$DOTNET_BIN" ]]; then
  echo "dotnet not found at $DOTNET_BIN" >&2
  exit 1
fi

if [[ ! -x "$GODOT_BIN" ]]; then
  echo "Godot not found at $GODOT_BIN" >&2
  exit 1
fi

"$DOTNET_BIN" build "$PROJECT_DIR/$ASSEMBLY_NAME.csproj"
"$GODOT_BIN" --headless --path "$PROJECT_DIR" --script "$PROJECT_DIR/tools/build_pck.gd"

mkdir -p "$MOD_OUTPUT_DIR"
rm -f "$MOD_OUTPUT_DIR/"*.dll "$MOD_OUTPUT_DIR/"*.pck
cp "$DLL_SOURCE" "$MOD_OUTPUT_DIR/"
cp "$PCK_SOURCE" "$MOD_OUTPUT_DIR/"

echo "MOD files copied to: $MOD_OUTPUT_DIR"
