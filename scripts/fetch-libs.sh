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
find "$TMP/unity/lib" -name '*.dll' -exec cp {} "$MANAGED/" \;

echo ">> Fetching BepInEx $BEPINEX_VERSION"
curl -sSL -o "$TMP/bepinex.zip" \
  "https://github.com/BepInEx/BepInEx/releases/download/v$BEPINEX_VERSION/BepInEx_linux_x64_$BEPINEX_VERSION.zip"
unzip -qo "$TMP/bepinex.zip" 'BepInEx/core/*' -d "$TMP/bepinex"
cp "$TMP"/bepinex/BepInEx/core/*.dll "$CORE/"

# SoftReferenceableAssets.dll is not part of ValheimGameLibs but appears in
# signatures of some Jotunn/Valheim APIs, so the compiler needs the type
# identities to exist. An empty stub with the right assembly name suffices -
# the mod never calls those APIs.
echo ">> Generating SoftReferenceableAssets reference stub"
STUB="$TMP/stub"
mkdir -p "$STUB"
cat > "$STUB/SoftReferenceableAssets.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>SoftReferenceableAssets</AssemblyName>
    <RootNamespace>SoftReferenceableAssets</RootNamespace>
    <Version>0.0.0</Version>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>
    <NoWarn>CS0169</NoWarn>
  </PropertyGroup>
</Project>
EOF
cat > "$STUB/Stub.cs" <<'EOF'
namespace SoftReferenceableAssets
{
    public struct AssetID
    {
        private int _a;
    }

    public struct SoftReference<T>
    {
        private object _handle;
    }
}
EOF
dotnet build "$STUB/SoftReferenceableAssets.csproj" -c Release -v quiet --nologo
cp "$STUB/bin/Release/netstandard2.0/SoftReferenceableAssets.dll" "$MANAGED/SoftReferenceableAssets.dll"
cp "$STUB/bin/Release/netstandard2.0/SoftReferenceableAssets.dll" "$PUB/SoftReferenceableAssets_publicized.dll"

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
