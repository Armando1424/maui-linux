# Stability gate — .NET 10 (Phase 5)

Date: 2026-11-11
Status: **SIGNED — both repositories pass every criterion**

## GtkSharp (`feature/net10` @ `a01614d4a`)

| Criterion | Target | Result | Status |
|:--|:--|:--|:--|
| Functional tests | ≥191, 0 failures | **244**, 0 failures | ✅ |
| Performance tests | 13 green | 13 green (Pi) | ✅ |
| Hand-written coverage | ≥30% | **30.31%** (7312/24128 lines) | ✅ |
| Coverage vs baseline | ≥21.3% | 30.31% (+8.9 points) | ✅ |
| Public API | no removals/renames | `PublicAPI/*.txt` (7 assemblies) unchanged; `netstandard2.0` target keeps the .NET 8 surface | ✅ |
| Workload from packages | clean SDK | workload E2E job (`dotnet new gtk`, run under Xvfb) | ✅ |
| CI | 3 consecutive green | `42884992156`, `42892361284`, `42893456682` | ✅ |

The extra coverage comes from KeyFile, Value/Variant, DateTime/Date,
IOChannel, Gdk.Rectangle/Pixbuf, TreeStore/ListStore, PtrArray and Builder
tests (53 new tests). GioSharp remains at 0%: the wrapper is generated and no
hand-written code lives there.

## maui-linux (`feature/net10` @ `a8bbe527e`)

| Criterion | Target | Result | Status |
|:--|:--|:--|:--|
| `BuildTasks.slnf` + `Gtk.slnf` | compile | compile, 0 errors | ✅ |
| Handler tests | ≥40 green | 65 functional + 6 performance | ✅ |
| Handler matrix | test or justified gap | 65 tests + 42 `[MissingMapper]` with reason | ✅ |
| Sample soak | 30 min stable | 30 min, **RSS +0.0 MB** (123.7 MB flat), CPU 56.4% (sample spinner) | ✅ |
| New warnings | none | 60 warnings (clean rebuild `--no-incremental`), same count as `90c03a3ed` and `73c7c7707` | ✅ |
| CI | 3 consecutive green | run `43107853189` attempts 1, 2 and 3 on the code commit; `43120816087` green on the signed commit | ✅ |

CI run `43107853189` also runs the performance tests and the 5 min soak as
**blocking** steps: 71 tests, soak +6.1 MB / 5.2% CPU.

## Extra evidence

- Startup on the Pi: median 9.1 s (limit 15 s), `eng/perf/startup.sh`.
- Benchmarks: `Measure` 12.75 µs / 480 B, `MeasureAndArrange` 13.66 µs / 528 B.
- Bindings: 1,000 updates below 24 MB.
- Disconnected handlers: ≤10% alive after GC.
- Baseline: `docs/baseline-net10.md`.

## Sign-off

Both repositories pass the gate on 2026-11-11:

- GtkSharp `feature/net10` @ `a01614d4a`: 244 functional tests, 13 performance
  tests, 30.31% hand-written coverage, API baseline unchanged, workload E2E
  green, run `43120736499` attempts 1–3 green.
- maui-linux `feature/net10` @ `a8bbe527e`: 65 functional + 6 performance
  tests, 30 minute soak with +0.0 MB, 3 consecutive green CI attempts on the
  code commit and a green run on the signed commit (`43120816087`).

Next phase: merge both `feature/net10` branches into `develop` (Phase 6).
