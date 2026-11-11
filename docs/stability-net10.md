# Stability gate — .NET 10 (Phase 5)

Date: 2026-11-11
Status: **maui-linux PASS — GtkSharp PASS with one exception (coverage)**

## GtkSharp (`feature/net10` @ `8f49dd8be`)

| Criterion | Target | Result | Status |
|:--|:--|:--|:--|
| Functional tests | ≥191, 0 failures | 191, 0 failures | ✅ |
| Performance tests | 13 green | 13 green (Pi) | ✅ |
| Hand-written coverage | ≥30% | **21.4%** (5158/24128 lines) | ❌ |
| Coverage vs baseline | ≥21.3% | 21.4% | ✅ |
| Public API | no removals/renames | `PublicAPI/*.txt` (7 assemblies) unchanged; `netstandard2.0` target keeps the .NET 8 surface | ✅ |
| Workload from packages | clean SDK | workload E2E job (`dotnet new gtk`, run under Xvfb) | ✅ |
| CI | 3 consecutive green | `42884992156`, `42892361284`, `42893456682` | ✅ |

Coverage by assembly: GLibSharp 31.9%, AtkSharp 48.1%, CairoSharp 28.4%,
GtkSharp 13.2%, PangoSharp 13.3%, GdkSharp 5.8%, GioSharp 0%.
Reaching 30% requires covering **2,080 more lines**; the planned 30% was an
aspirational target, not a measured ceiling.

## maui-linux (`feature/net10` @ `f94963a9a`)

| Criterion | Target | Result | Status |
|:--|:--|:--|:--|
| `BuildTasks.slnf` + `Gtk.slnf` | compile | compile, 0 errors | ✅ |
| Handler tests | ≥40 green | 65 functional + 6 performance | ✅ |
| Handler matrix | test or justified gap | 65 tests + 42 `[MissingMapper]` with reason | ✅ |
| Sample soak | 30 min stable | 30 min, **RSS +0.0 MB** (123.7 MB flat), CPU 56.4% (sample spinner) | ✅ |
| New warnings | none | 60 warnings (clean rebuild `--no-incremental`), same count as `90c03a3ed` and `73c7c7707` | ✅ |
| CI | 3 consecutive green | run `43107853189` attempts 1, 2 and 3 on `f94963a9a` | ✅ |

CI run `43107853189` also runs the performance tests and the 5 min soak as
**blocking** steps: 71 tests, soak +6.1 MB / 5.2% CPU.

## Extra evidence

- Startup on the Pi: median 9.1 s (limit 15 s), `eng/perf/startup.sh`.
- Benchmarks: `Measure` 12.75 µs / 480 B, `MeasureAndArrange` 13.66 µs / 528 B.
- Bindings: 1,000 updates below 24 MB.
- Disconnected handlers: ≤10% alive after GC.
- Baseline: `docs/baseline-net10.md`.

## Decision needed

The only unmet criterion is the GtkSharp hand-written coverage target (21.4%
vs 30%). Two options:

1. **Revise the target** to the baseline (no regression, 21.4%) and sign the
   gate; the 30% figure was estimated before knowing the size of the generated
   marshalling surface.
2. **Keep 30%** and postpone the merge until ~2,080 additional lines are
   covered (model/builder/clipboard/GLib.Value tests and GioSharp coverage).

Everything else in the gate is green.
