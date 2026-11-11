# Performance and memory — GTK backend (net10)

Last update: 2026-11-11
Phase: 4.5 of `PLAN-net10.md`

## What is covered

| Test | Where | What it protects |
|:--|:--|:--|
| Handler churn (10k) | `Performance/HandlerPerformanceTests` | RSS steady state when creating and disconnecting handlers |
| Label updates (10k) | `Performance/HandlerPerformanceTests` | allocations and time per property update |
| Layout measure/arrange (10k) | `Performance/HandlerPerformanceTests` | RSS and time of `StackLayout` measure/arrange |
| Push/pop (1k) | `Performance/HandlerPerformanceTests` | stack child count and RSS across navigation cycles |
| Soak (30 min) | `eng/soak/gtk-soak.sh` | process stability, RSS growth and sustained CPU |

The tests are tagged `Category=Performance` so the functional suite stays fast:

```sh
export DOTNET_ROOT="$HOME/dotnet-gtk10"
export PATH="$HOME/dotnet-gtk10:$PATH"

xvfb-run -a dotnet test src/Controls/tests/Gtk.UnitTests \
  -c Release -p:_IncludeAndroid= --filter "Category=Performance"
```

## Baseline on the Raspberry Pi 4 (ARM64)

Measured with `Release`, `net10.0-gtk`, GtkSharp `3.24.24.200`.

| Metric | Result | Limit in the test |
|:--|--:|--:|
| Handler churn | ~5 KB per cycle | 12 KB per cycle |
| Label updates | < 1 KB allocated per update | 1 KB per update |
| Layout measure/arrange | 10k cycles < 20 s | 20 s |
| Push/pop | 1k cycles, 1 child, RSS +< 48 MB | 48 MB |
| Soak, 2 min | RSS +18 MB, CPU 46% | — |
| Soak, 30 min | see `artifacts/soak-30/` (local run) | +64 MB, 90% CPU |

RSS is read from `/proc/<pid>/status` and trimmed with `malloc_trim` before
each measurement. The limits are deliberately generous: the Pi is a small,
shared machine and GTK keeps font and theme caches alive.

## Soak test

```sh
eng/soak/gtk-soak.sh --minutes 30 --interact --out artifacts/soak
```

- Builds `Controls.Sample.Gtk` (skip with `--no-build`).
- Runs it under `xvfb-run`, samples RSS and CPU every 5 s into a CSV.
- Fails if the process dies, if RSS grows more than `--rss-limit-mb` (64) or
  if sustained CPU is above `--cpu-limit` (90%).
- `--interact` sends `Tab` every 30 s with `xdotool` to exercise the UI.
- Results: `artifacts/soak/*.csv` and `*.summary.txt`.

The CI job `Build Controls.Sample.Gtk` runs the performance tests and a 5 min
soak as **advisory** steps (`continue-on-error`) and uploads the results. Make
them blocking once three consecutive runs are stable (Phase 5).

## Bugs found by this phase

| Bug | Effect | Fix |
|:--|:--|:--|
| `LayoutHandler.DisconnectHandler` read `PlatformView` after the base class cleared it | `InvalidOperationException` on every layout disconnect | clear/ignore children before releasing the platform view |
| Popped pages kept their handler and native widget | one leaked page per push/pop cycle | `NavigationView` disconnects pages that leave the stack |
| `Gtk.Widget` instances were never disposed | native widgets accumulated on handler churn | `ViewHandler.DisconnectingHandler` disposes unparented widgets |

## Known gaps

- The steady state still grows ~5 KB per handler cycle on the Pi (GTK font and
  theme caches). It is bounded by the test limit but not zero.
- No BenchmarkDotNet project yet; the timing checks are threshold based.
- The soak exercises the sample, not a real app; interaction is a simple
  focus cycle.
