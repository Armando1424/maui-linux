#!/usr/bin/env bash
# Measures the time from process start to the first mapped window.
#
# ApplicationExtensions prints MAUI_STARTUP_MS when MAUI_STARTUP_PROBE=1.
#
# Usage: eng/perf/startup.sh [--runs 3] [--limit-ms 6000] [--no-build]
set -uo pipefail

RUNS=3
LIMIT_MS=15000
BUILD=1

while [ $# -gt 0 ]; do
  case "$1" in
    --runs) RUNS="$2"; shift 2 ;;
    --limit-ms) LIMIT_MS="$2"; shift 2 ;;
    --no-build) BUILD=0; shift ;;
    -h|--help) sed -n '2,9p' "$0"; exit 0 ;;
    *) echo "Unknown option: $1" >&2; exit 2 ;;
  esac
done

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

if [ "$BUILD" -eq 1 ]; then
  dotnet build src/Controls/samples/Controls.Sample.Gtk/Controls.Sample.Gtk.csproj \
    -c Release -p:_IncludeAndroid= >/dev/null || exit 2
fi

SAMPLE="$(find src/Controls/samples/Controls.Sample.Gtk/bin/Release -name 'Controls.Sample.Gtk.dll' | head -1)"
[ -z "$SAMPLE" ] && { echo "The sample is not built." >&2; exit 2; }

cd "$(dirname "$SAMPLE")"

run_once() {
  local log
  log="$(mktemp)"

  MAUI_STARTUP_PROBE=1 xvfb-run -a dotnet Controls.Sample.Gtk.dll >"$log" 2>&1 &
  local pid=$!
  local line=""

  for _ in $(seq 1 80); do
    line="$(grep -m1 'MAUI_STARTUP_MS=' "$log" || true)"
    [ -n "$line" ] && break
    sleep 0.25
  done

  kill "$pid" 2>/dev/null
  wait "$pid" 2>/dev/null
  rm -f "$log"
  echo "$line"
}

declare -a TIMES=()

for i in $(seq 1 "$RUNS"); do
  LINE="$(run_once)"
  MS="$(echo "$LINE" | grep -oE 'MAUI_STARTUP_MS=[0-9]+' | cut -d= -f2)"

  if [ -z "$MS" ]; then
    echo "Run $i: no MAUI_STARTUP_MS line found." >&2
    exit 1
  fi

  echo "Run $i: ${MS} ms"
  TIMES+=("$MS")
done

MEDIAN="$(printf '%s\n' "${TIMES[@]}" | sort -n | awk '{a[NR]=$1} END {print a[int((NR+1)/2)]}')"

echo "Startup median: ${MEDIAN} ms (runs: ${TIMES[*]})"

if [ "$MEDIAN" -gt "$LIMIT_MS" ]; then
  echo "FAIL: startup ${MEDIAN} ms > ${LIMIT_MS} ms" >&2
  exit 1
fi

echo "OK: startup within ${LIMIT_MS} ms"
