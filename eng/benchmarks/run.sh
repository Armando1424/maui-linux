#!/usr/bin/env bash
# Runs the GTK benchmarks. BenchmarkDotNet rebuilds the project itself, so the
# Android target framework has to be disabled in Directory.Build.Override.props
# for the duration of the run (the same sed the CI uses).
#
# Usage: eng/benchmarks/run.sh [BenchmarkDotNet arguments]
#   eng/benchmarks/run.sh --filter "*LayoutBenchmarks*" --job short
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

BACKUP="$(mktemp)"
cp Directory.Build.Override.props "$BACKUP"
trap 'mv "$BACKUP" Directory.Build.Override.props' EXIT

sed -i 's/_IncludeAndroid>true/_IncludeAndroid>/g' Directory.Build.Override.props

mkdir -p artifacts/benchmarks

EXTRA=()
if [[ " $* " != *" --buildTimeout "* ]]; then
  EXTRA+=(--buildTimeout 900)
fi

xvfb-run -a dotnet run --project src/Controls/tests/Gtk.Benchmarks/Gtk.Benchmarks.csproj \
  -c Release -- "$@" "${EXTRA[@]}" --artifacts artifacts/benchmarks
