#!/usr/bin/env bash
# Long-running soak test for the GTK backend.
#
# Launches Controls.Sample.Gtk under Xvfb, samples RSS and CPU every few
# seconds and fails when the process dies or when memory grows without bound.
#
# Usage:
#   eng/soak/gtk-soak.sh [--minutes 30] [--sample-seconds 5] [--no-build]
#                        [--interact] [--out artifacts/soak]
#
# Exit codes: 0 = stable, 1 = failure, 2 = configuration/build error.
set -uo pipefail

MINUTES=30
SAMPLE_SECONDS=5
BUILD=1
INTERACT=0
OUT_DIR="artifacts/soak"
RSS_LIMIT_MB=64
CPU_LIMIT_PERCENT=90

while [ $# -gt 0 ]; do
  case "$1" in
    --minutes) MINUTES="$2"; shift 2 ;;
    --sample-seconds) SAMPLE_SECONDS="$2"; shift 2 ;;
    --no-build) BUILD=0; shift ;;
    --interact) INTERACT=1; shift ;;
    --out) OUT_DIR="$2"; shift 2 ;;
    --rss-limit-mb) RSS_LIMIT_MB="$2"; shift 2 ;;
    --cpu-limit) CPU_LIMIT_PERCENT="$2"; shift 2 ;;
    -h|--help) sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "Unknown option: $1" >&2; exit 2 ;;
  esac
done

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

if [ "$BUILD" -eq 1 ]; then
  echo "==> Building the GTK sample"
  dotnet build src/Controls/samples/Controls.Sample.Gtk/Controls.Sample.Gtk.csproj \
    -c Release -p:_IncludeAndroid= >/dev/null || exit 2
fi

SAMPLE="$(find src/Controls/samples/Controls.Sample.Gtk/bin/Release -name 'Controls.Sample.Gtk.dll' | head -1)"
if [ -z "$SAMPLE" ]; then
  echo "The sample was not built. Run without --no-build first." >&2
  exit 2
fi

mkdir -p "$OUT_DIR"
OUT_DIR="$(cd "$OUT_DIR" && pwd)"
CSV="$OUT_DIR/soak-$(date +%Y%m%d-%H%M%S).csv"
LOG="$OUT_DIR/soak.log"
echo "elapsed_s,rss_kb,cpu_ticks" >"$CSV"
: >"$LOG"

echo "==> Sample: $SAMPLE"
echo "==> Duration: ${MINUTES} min (sample every ${SAMPLE_SECONDS}s)"

cd "$(dirname "$SAMPLE")"
xvfb-run -a dotnet Controls.Sample.Gtk.dll >>"$LOG" 2>&1 &
WRAPPER_PID=$!

# The dotnet process is a child of xvfb-run (which runs a shell).
APP_PID=""
for _ in $(seq 1 60); do
  APP_PID="$(pgrep -P "$WRAPPER_PID" -f 'Controls.Sample.Gtk.dll' 2>/dev/null | head -1)"
  [ -n "$APP_PID" ] && break
  APP_PID="$(pgrep -f 'Controls.Sample.Gtk.dll' 2>/dev/null | grep -v "^$WRAPPER_PID$" | head -1)"
  [ -n "$APP_PID" ] && break
  sleep 0.5
done

if [ -z "$APP_PID" ]; then
  echo "The sample did not start. Log: $LOG" >&2
  tail -20 "$LOG" >&2
  kill "$WRAPPER_PID" 2>/dev/null
  exit 1
fi

APP_DISPLAY="$(tr '\0' '\n' < "/proc/$APP_PID/environ" 2>/dev/null | awk -F= '/^DISPLAY=/{print $2}')"
echo "==> PID: $APP_PID (DISPLAY=${APP_DISPLAY:-none})" 
HZ="$(getconf CLK_TCK)"
STARTED=$SECONDS
DURATION=$((MINUTES * 60))
NEXT_INTERACTION=0
DIED=0

while [ $((SECONDS - STARTED)) -lt "$DURATION" ]; do
  if [ ! -r "/proc/$APP_PID/status" ]; then
    DIED=1
    break
  fi

  ELAPSED=$((SECONDS - STARTED))
  RSS="$(awk '/VmRSS/ {print $2}' "/proc/$APP_PID/status" 2>/dev/null)"
  TICKS="$(awk '{print $14 + $15}' "/proc/$APP_PID/stat" 2>/dev/null)"
  [ -z "$RSS" ] || [ -z "$TICKS" ] && break

  echo "$ELAPSED,$RSS,$TICKS" >>"$CSV"

  if [ "$INTERACT" -eq 1 ] && [ "$ELAPSED" -ge "$NEXT_INTERACTION" ]; then
    # Exercise the UI: cycle focus and activate the focused widget.
    [ -n "$APP_DISPLAY" ] && DISPLAY="$APP_DISPLAY" xdotool key Tab 2>/dev/null
    NEXT_INTERACTION=$((ELAPSED + 30))
  fi

  sleep "$SAMPLE_SECONDS"
done

if [ "$DIED" -eq 1 ] || [ ! -r "/proc/$APP_PID/status" ]; then
  echo "==> The sample died after $((SECONDS - STARTED))s" >&2
  tail -30 "$LOG" >&2
  kill "$WRAPPER_PID" 2>/dev/null
  exit 1
fi

kill "$APP_PID" 2>/dev/null
sleep 1
kill -9 "$APP_PID" 2>/dev/null
kill "$WRAPPER_PID" 2>/dev/null

python3 - "$CSV" "$RSS_LIMIT_MB" "$CPU_LIMIT_PERCENT" "$HZ" <<'PY'
import csv
import sys

path, rss_limit_mb, cpu_limit, hz = sys.argv[1], float(sys.argv[2]), float(sys.argv[3]), int(sys.argv[4])
rows = []
with open(path, newline="") as handle:
    for row in csv.DictReader(handle):
        rows.append((float(row["elapsed_s"]), float(row["rss_kb"]), float(row["cpu_ticks"])))

if len(rows) < 4:
    print(f"Not enough samples ({len(rows)}).", file=sys.stderr)
    sys.exit(1)

# Compare the first and the last quarter so warmup caches do not count.
quarter = max(1, len(rows) // 4)
baseline = sum(r[1] for r in rows[:quarter]) / quarter
final = sum(r[1] for r in rows[-quarter:]) / quarter
growth_mb = (final - baseline) / 1024.0

elapsed = rows[-1][0] - rows[0][0]
cpu_seconds = (rows[-1][2] - rows[0][2]) / hz
cpu_percent = (cpu_seconds / elapsed * 100.0) if elapsed > 0 else 0.0

summary = (
    f"Soak summary: samples={len(rows)} duration={elapsed:.0f}s "
    f"rss_start={baseline/1024:.1f}MB rss_end={final/1024:.1f}MB "
    f"growth={growth_mb:+.1f}MB cpu={cpu_percent:.1f}% "
    f"max_rss={max(r[1] for r in rows)/1024:.1f}MB"
)
print(summary)

failed = False
if growth_mb > rss_limit_mb:
    print(f"FAIL: RSS grew {growth_mb:.1f}MB (limit {rss_limit_mb:.0f}MB).", file=sys.stderr)
    failed = True
if cpu_percent > cpu_limit:
    print(f"FAIL: sustained CPU {cpu_percent:.1f}% (limit {cpu_limit:.0f}%).", file=sys.stderr)
    failed = True

with open(path.replace(".csv", ".summary.txt"), "w") as handle:
    handle.write(summary + "\n")

sys.exit(1 if failed else 0)
PY

RC=$?
if [ "$RC" -eq 0 ]; then
  echo "==> Soak passed. Results in $OUT_DIR"
else
  echo "==> Soak failed. Results in $OUT_DIR" >&2
fi
exit "$RC"
