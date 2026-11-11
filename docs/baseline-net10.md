# Baseline .NET 10 (Phase 0)

Last update: 2026-11-11

This is the reference point for the .NET 10 migration. Phases 5 and 6 compare
against these numbers.

## Repositories

| Repo | Branch | Commit | Notes |
|:--|:--|:--|:--|
| `Armando1424/GtkSharp` | `feature/net10` | `8f49dd8be` | `.NET 10` libraries, packages `3.24.24.200`/`3.24.24.201` |
| `Armando1424/maui-linux` | `feature/net10` | `73c7c7707` | GTK backend on `net10.0-gtk` |
| `Armando1424/GtkSharp` | `develop`/`master` | `c01f5f97d` | untouched |
| `Armando1424/maui-linux` | `develop`/`master` | `b90c5fe68` | untouched |

## Toolchain

| Item | Version |
|:--|:--|
| .NET SDK (Pi and CI) | `10.0.403` |
| GtkSharp packages | `3.24.24.200` |
| Workload band | `10.0.400` |
| GtkSharp workload manifest | `GtkSharp.NET.Sdk.Gtk.Manifest-10.0.400` |
| TFM | `net10.0-gtk` |

## Tests

| Suite | Result |
|:--|:--|
| GtkSharp functional | 191 tests |
| GtkSharp performance | 13 tests (BenchmarkDotNet project) |
| maui-linux handlers | 65 functional tests |
| maui-linux performance | 6 tests (`Category=Performance`) |
| maui-linux soak | 30 min, RSS +0.8 MB, ~120 MB plateau |
| maui-linux startup | median 9.1 s on the Pi |

## CI

| Repo | Run | Result |
|:--|:--|:--|
| GtkSharp | `42893456682` | green |
| maui-linux | `43090188507` | green (69 tests, soak +6.2 MB) |
| maui-linux | `43070829000` | green |
| maui-linux | `43036556824` | green |

## Known measured limits

- GtkSharp hand-written code coverage: 21.3% (Phase 5 target: 30%).
- Soak limits: +64 MB RSS, 90% CPU.
- Startup limit: 15 s on the Pi.
- Handler churn: ~5 KB per cycle (limit 12 KB).
- Bindings: 1,000 updates below 24 MB of RSS growth.

See `docs/performance-gtk.md` for how each number is produced.
