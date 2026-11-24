#!/usr/bin/env bash
set -euo pipefail

if [[ -z ${DISPLAY:-} ]]; then
    echo 'Run this smoke test in an X session or with xvfb-run.' >&2
    exit 1
fi

test_dir=$(cd -- "$(dirname -- "$0")" && pwd)
repo=$(cd -- "$test_dir/../.." && pwd)
scratch=$(mktemp -d)
trap 'rm -rf -- "$scratch"' EXIT
mkdir -p "$scratch/home" "$scratch/fixture"

dotnet build "$repo/linux/Gral.Mono.csproj" -c Release \
    -p:BaseIntermediateOutputPath="$scratch/obj/" \
    '-p:DefaultItemExcludesInProjectFolder=obj/**%3B**/.*/**' -o "$scratch/bin" "$@"
mcs -debug -r:System.Windows.Forms -r:System.Drawing -r:"$scratch/bin/GRAL_GUI.exe" \
    -out:"$scratch/bin/GralMonoSmoke.exe" "$test_dir/Program.cs"

export XAUTHORITY="${XAUTHORITY:-$HOME/.Xauthority}"
export HOME="$scratch/home" XDG_CONFIG_HOME="$scratch/home/.config" \
    XDG_CACHE_HOME="$scratch/home/.cache" XDG_DATA_HOME="$scratch/home/.local/share"
export LC_ALL=C
cd -- "$scratch/fixture"
timeout 45s mono --debug "$scratch/bin/GralMonoSmoke.exe"
