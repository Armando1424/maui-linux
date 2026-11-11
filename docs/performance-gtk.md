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
| Startup | `eng/perf/startup.sh` | process start → first mapped window |
| Layout benchmarks | `src/Controls/tests/Gtk.Benchmarks` | measure/arrange and label updates with BenchmarkDotNet |
| Collectability | `DisconnectedHandlersAreCollectable` | disconnected handlers are freed (no unexpected finalization) |
| Bindings | `BindingUpdatesKeepMemoryStable` | 1,000 binding updates without RSS growth |

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
| Soak, 30 min | RSS +0.8 MB (plateau at ~120 MB), CPU 44% | +64 MB, 90% CPU |
| Startup, Pi (3 runs) | median 9.1 s (8.2–11.5 s) | 15 s |
| Layout measure (BDN, ShortRun) | 12.75 µs, 480 B allocated | — |
| Layout measure+arrange (BDN) | 13.66 µs, 528 B allocated | — |
| Bindings (1k) | RSS stable | 24 MB |
| Disconnected objects | < 10% still alive after GC | 10% |

The 30 minute soak on the Pi stabilises at ~120 MB: the first minutes are
warmup (56 MB → 120 MB) and the last quarter of the run grows only 0.8 MB. Use
`--minutes 30` (or more) for meaningful numbers; a 1 minute run only measures
warmup. The 44% CPU comes from the always-running `ActivityIndicator` in
`Controls.Sample.Gtk`, not from an idle loop.

RSS is read from `/proc/<pid>/status` and trimmed with `malloc_trim` before
each measurement. The limits are deliberately generous: the Pi is a small,
shared machine and GTK keeps font and theme caches alive.

## Benchmarks

```sh
eng/benchmarks/run.sh --filter "*LayoutBenchmarks*" --job short
eng/benchmarks/run.sh --filter "*HandlerBenchmarks*" --job short
```

The script disables the Android target framework in
`Directory.Build.Override.props` for the run (BenchmarkDotNet rebuilds the
project without the `-p:_IncludeAndroid=` property) and restores it afterwards.
Results go to `artifacts/benchmarks`. The Pi baseline (ShortRun, 3 iterations)
for `LayoutBenchmarks` is 12.75 µs for measure and 13.66 µs for
measure+arrange, with less than 600 B allocated per call.

## Startup

```sh
eng/perf/startup.sh --runs 3 --limit-ms 15000
```

`ApplicationExtensions.CreatePlatformWindow` prints `MAUI_STARTUP_MS` when
`MAUI_STARTUP_PROBE=1`; the script reports the median and fails above the
limit. On the Pi the baseline is ~9 s; the CI runner is faster.

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
soak as **blocking** steps (three stable runs were reached) and uploads the
soak results. If a threshold turns out to be too tight for the shared runner,
raise the threshold before making the step advisory again.

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
  focus cycle. A page without animations is needed to measure real idle CPU.
