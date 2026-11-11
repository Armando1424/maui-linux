# GTK 3 versus GTK 4

Last update: 2026-11-11

## Short answer

This fork targets **GTK 3**. The `net10.0-gtk` workload and every
`src/Core/src/Platform/Gtk` widget are built on **GtkSharp 3.24**, which binds
GTK 3.24. GTK 4 is a different major version with a different binding library,
so it is a new backend, not a version bump.

## Why not GTK 4

| Topic | GTK 3 (current) | GTK 4 |
|:--|:--|:--|
| Binding | GtkSharp `3.24.24.x` | `GirCore.Gtk-4.0` (different API and project) |
| Widget model | `Gtk.Container`, `Gtk.Bin`, `Gtk.Box` packing | no `Container`; layout managers and `Gtk.Box.Append` |
| Drawing | `Gtk.DrawingArea` + Cairo `OnDrawn` | snapshots (`GtkSnapshot`) and `Gtk.DrawingArea.SetDrawFunc` |
| CSS | `CssProvider` per widget (`StyleContext.AddProvider`) | global `Gtk.StyleContext.AddProviderForDisplay` |
| Fonts | `Widget.ModifyFont` | CSS / `Pango` attributes on labels |
| Events | `StateSet`, `ButtonPressEvent`, … | controllers (`Gtk.GestureClick`, `EventControllerKey`, …) |
| Widgets used here | `Notebook`, `ListBox`, `ScrolledWindow`, `ComboBox`, `Calendar` | `Notebook` exists; `ComboBox` changed; `Calendar` moved out of GTK |
| Availability | Raspberry Pi OS bookworm ships GTK 3 (`libgtk-3-0`) | `libgtk-4-1` is in the repo but not installed by default |

Porting would mean rewriting every platform view and handler (buttons, entries,
layouts, navigation, toolbar, collection views, tabs, tables…) against a new
binding, plus a second workload (`net10.0-gtk4`) and a second CI matrix. That is
a phase of its own, not a change inside the GTK 3 backend.

## What would be needed

1. A GTK 4 binding: `GirCore.Gtk-4.0` (0.8.x at the time of writing).
2. A parallel `Platform/Gtk4` folder and `*Handler.Gtk4.cs` files (the shared
   handler logic can stay).
3. A `Gtk4` workload/manifest and a `net10.0-gtk4` TFM.
4. CI jobs for both backends, and documentation of which controls are
   supported on each.

A local clone of GirCore lives in `~/proyectos/gir.core` (commit `b286a4b`)
with its own `CONTEXT.local.md`; it is the reference for the future GTK 4
backend and is not part of the net10 migration.

Until then, the GTK 3 backend is the supported Linux backend for this fork.
