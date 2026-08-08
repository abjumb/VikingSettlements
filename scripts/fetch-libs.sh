#!/usr/bin/env bash
# Fetches reference assemblies so the mod can be compiled without a local
# Valheim installation (e.g. in CI or a container).
#
# It assembles a minimal Valheim install layout under vendor/valheim:
#   valheim_Data/Managed/                    - game + Unity assemblies
#   valheim_Data/Managed/publicized_assemblies/ - publicized game assemblies
#   BepInEx/core/                            - BepInEx 5 core assemblies
# and writes Environment.props pointing VALHEIM_INSTALL at it.
#
# Sources:
#   ValheimGameLibs (NuGet)     - already publicized+stripped game assemblies
#   UnityEngine.Modules (NuGet) - UnityEngine reference assemblies
#   BepInEx (GitHub releases)   - BepInEx 5 core
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENDOR="$ROOT/vendor/valheim"
MANAGED="$VENDOR/valheim_Data/Managed"
PUB="$MANAGED/publicized_assemblies"
CORE="$VENDOR/BepInEx/core"

VGL_VERSION="${VGL_VERSION:-0.221.4}"
UNITY_VERSION="${UNITY_VERSION:-2021.3.33}"
BEPINEX_VERSION="${BEPINEX_VERSION:-5.4.23.3}"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

mkdir -p "$MANAGED" "$PUB" "$CORE"

echo ">> Fetching ValheimGameLibs $VGL_VERSION (publicized game assemblies)"
curl -sSL -o "$TMP/vgl.nupkg" \
  "https://api.nuget.org/v3-flatcontainer/valheimgamelibs/$VGL_VERSION/valheimgamelibs.$VGL_VERSION.nupkg"
unzip -qo "$TMP/vgl.nupkg" 'lib/*' -d "$TMP/vgl"
chmod -R u+rw "$TMP/vgl"
cp "$TMP"/vgl/lib/*.dll "$MANAGED/"

# The JotunnLib build props reference the game assemblies via
# publicized_assemblies/<name>_publicized.dll - ValheimGameLibs assemblies are
# already publicized, so copies under that naming satisfy the references.
for f in "$MANAGED"/assembly_*.dll; do
  base="$(basename "$f" .dll)"
  cp "$f" "$PUB/${base}_publicized.dll"
done

echo ">> Fetching UnityEngine.Modules $UNITY_VERSION"
curl -sSL -o "$TMP/unity.nupkg" \
  "https://api.nuget.org/v3-flatcontainer/unityengine.modules/$UNITY_VERSION/unityengine.modules.$UNITY_VERSION.nupkg"
unzip -qo "$TMP/unity.nupkg" 'lib/*' -d "$TMP/unity"
# The nupkg stores these entries with mode 000; unzip preserves that, and an
# unprivileged user then cannot read what it just extracted. (find -exec cp
# would also swallow the failures.)
chmod -R u+rw "$TMP/unity"
find "$TMP/unity/lib" -name '*.dll' -exec cp {} "$MANAGED/" \;
test -f "$MANAGED/UnityEngine.dll" \
  || { echo "ERROR: UnityEngine modules failed to copy into $MANAGED" >&2; exit 1; }

# UnityEngine.UI (uGUI) ships separately from the engine modules; the API
# surface the mod uses (Button, onClick) is stable across Unity versions and
# at runtime the game's own UnityEngine.UI.dll resolves by name.
UGUI_VERSION="${UGUI_VERSION:-2020.3.21}"
echo ">> Fetching Unity3D.UnityEngine.UI $UGUI_VERSION"
curl -sSL -o "$TMP/ugui.nupkg" \
  "https://api.nuget.org/v3-flatcontainer/unity3d.unityengine.ui/$UGUI_VERSION/unity3d.unityengine.ui.$UGUI_VERSION.nupkg"
unzip -qo "$TMP/ugui.nupkg" 'lib/*' -d "$TMP/ugui"
chmod -R u+rw "$TMP/ugui"
find "$TMP/ugui/lib" -name 'UnityEngine.UI.dll' -exec cp {} "$MANAGED/" \;
test -f "$MANAGED/UnityEngine.UI.dll" \
  || { echo "ERROR: UnityEngine.UI failed to copy into $MANAGED" >&2; exit 1; }

echo ">> Fetching BepInEx $BEPINEX_VERSION"
curl -sSL -o "$TMP/bepinex.zip" \
  "https://github.com/BepInEx/BepInEx/releases/download/v$BEPINEX_VERSION/BepInEx_linux_x64_$BEPINEX_VERSION.zip"
unzip -qo "$TMP/bepinex.zip" 'BepInEx/core/*' -d "$TMP/bepinex"
cp "$TMP"/bepinex/BepInEx/core/*.dll "$CORE/"

# SoftReferenceableAssets.dll and Splatform.dll are not part of
# ValheimGameLibs but appear in signatures of some Jotunn/Valheim APIs
# (e.g. Minimap.AddPin takes a Splatform.PlatformUserID), so the compiler
# needs the type identities to exist. Empty stubs with the right assembly
# names suffice - the mod never touches those types directly.
build_stub() {
  local name="$1" source="$2"
  echo ">> Generating $name reference stub"
  local stub="$TMP/stub-$name"
  mkdir -p "$stub"
  cat > "$stub/$name.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>$name</AssemblyName>
    <Version>0.0.0</Version>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>
    <NoWarn>CS0169</NoWarn>
  </PropertyGroup>
</Project>
EOF
  printf '%s\n' "$source" > "$stub/Stub.cs"
  dotnet build "$stub/$name.csproj" -c Release -v quiet --nologo
  cp "$stub/bin/Release/netstandard2.0/$name.dll" "$MANAGED/$name.dll"
  cp "$stub/bin/Release/netstandard2.0/$name.dll" "$PUB/${name}_publicized.dll"
}

build_stub SoftReferenceableAssets 'namespace SoftReferenceableAssets
{
    public struct AssetID
    {
        private int _a;
    }

    public struct SoftReference<T>
    {
        private object _handle;
    }
}'

build_stub Splatform 'namespace Splatform
{
    public struct PlatformUserID
    {
        private ulong _id;
    }
}'

# cp onto a pre-existing file keeps the destination's old mode, so a vendor
# tree populated by an older run of this script may still carry mode-000
# files; normalize everything we just assembled.
chmod -R u+rw "$VENDOR"

echo ">> Writing Environment.props"
cat > "$ROOT/Environment.props" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <VALHEIM_INSTALL>$VENDOR</VALHEIM_INSTALL>
  </PropertyGroup>
</Project>
EOF

echo ">> Done. Build with: dotnet build VikingSettlements.sln"
