# GTK 4 port (experiment)

Last update: 2026-11-22

## Summary

A probe branch (`feature/gtk4-probe`, worktree outside the repo) ports the whole
GTK platform layer to **GtkSharp's GTK 4 bindings** (`gtk4-2-bindings`, the
second of the four upstream port PRs). Result:

```text
Core.csproj            0 errors
Controls.Core.csproj   0 errors
Controls.Sample.Gtk    0 errors, runs under Xvfb with GTK 4.18
```

The sample renders the layout, colours and fonts (see the capture in the
`gtk4-maui` spike). This document records how the port works and what is still
missing; [`gtk-version.md`](gtk-version.md) explains the GTK 3 vs GTK 4
differences.

Nothing of this is merged: `master` keeps targeting GTK 3 through the
`net10.0-gtk` workload. The work lives in the probe branch until the upstream
bindings are released.

## How the port is built

The `net10.0-gtk` TFM comes from the GtkSharp workload, which references the
GTK 3.24 assemblies. The probe replaces them without touching the workload:

```xml
<ItemGroup Condition="$(TargetFramework.Contains('-gtk'))">
  <FrameworkReference Remove="GtkSharp" />
  <PackageReference Remove="GtkSharp" />
  <Reference Include="GtkSharp"><HintPath>$(Gtk4Dir)GtkSharp.dll</HintPath></Reference>
  <!-- …one Reference per GTK assembly, including Graphene/Gsk/Adwaita -->
</ItemGroup>
```

That swap is probe-only and lives in its own commit, so it can be dropped once a
`net10.0-gtk4` workload exists.

## What changed in the platform layer

| GTK 3 | GTK 4 |
|:--|:--|
| `OnGetPreferredWidth/Height`, `OnGetPreferred*For*`, `OnAdjustSizeRequest`, `OnGetRequestMode` | `OnMeasure(Orientation, forSize, out min, out natural, …)` |
| `OnDrawn(Cairo.Context)` | `OnSnapshot(Gtk.Snapshot)` |
| `OnAdded` / `OnRemoved` | `Add` / `Remove` overrides |
| `base.OnSizeAllocated(...)` in a custom layout | `RaiseSizeAllocated(...)` only |
| `Gtk.Menu`, `MenuBar`, `MenuItem`, `SeparatorMenuItem` | `GLib.Menu` model + `Gtk.PopoverMenuBar` + `Gio.SimpleAction` |
| `WindowType`, `delete-event`, `Window.Move`, `Window.Resize` | none; `close-request`; no move; `SetDefaultSize` / `SetSizeRequest` |
| `StyleContext` per-widget providers, `ModifyFont` | one `CssProvider` per display + class per widget (`MauiCss`) |
| `ComboBox` with icons, `Calendar` in a `Popup` window, seat grabs | own button, `Gtk.Calendar` + `GLib.DateTime`, no grabs |
| `Gdk.Screen`, `Gdk.Visual`, `Gdk.Atom`, `Gdk.Threads`, event masks | removed; display, monitors, `Gdk.Clipboard`, controllers |
| `Button.Image`, `ListBox.Children`, `Notebook.NotebookChild` | `Button.Child`/`IconName`, tracked rows, `PageNum`/`RemovePage`/`InsertPage` |

## Findings worth keeping

1. **The binding's compat layer carries a lot of the port.** `Source/Libs/GtkSharp/Compat`
   (10 files, 2 533 lines) reimplements `Gtk.Container` with `Add`/`Remove`,
   `ShowAll`, `PackStart`, `Gtk.EventBox` and the `OnDrawn`/`OnSizeAllocated`
   virtuals on top of GTK 4. Subclasses of `Gtk.Container` therefore compile with
   few changes.
2. **The compat `Container` re-places children.** Its `OnSizeAllocated` calls
   `AllocateChildren()`, which puts every child back at `(0, 0)` with its natural
   size. A custom layout that arranges children itself **must not call
   `base.OnSizeAllocated`** — only `RaiseSizeAllocated` to publish the size.
3. **`ContentView` used to be a `Gtk.Bin`.** Moving it to `Container` lost the
   "child fills the allocation" behaviour, which left the `ScrollView` at its
   natural height (46 px). It is restored with
   `protected override bool ChildrenFillAllocation => true;`.
4. **GTK 4 has no per-widget style providers.** Theming has to go through one
   `CssProvider` on the display; `MauiCss` gives every widget a unique class and
   rebuilds the sheet when a property changes.

## Known limitations

- **Hot resize:** GTK 4 has no API. `SetSizeRequest` sets a minimum, not an exact
  size; `SetDefaultSize` only applies before mapping.
- **Blocking a close:** this binding exposes `close-request` as `void`, so the
  close cannot be cancelled (GTK 4 itself allows it).
- **Clipboard:** stubbed; GTK 4 uses `Gdk.Clipboard` with an async API.
- **`MauiCss` rebuilds the whole sheet** on every property change. Correct, but
  it should batch changes.
- **Fonts:** `UpdateFont` emits family/size/weight/style as CSS; `MauiFont`
  resources are not registered yet.
- **Menus:** the `GLib.Menu` model is rebuilt on change and has not been
  exercised at runtime.

## How to reproduce

```bash
dotnet build src/Controls/samples/Controls.Sample.Gtk/Controls.Sample.Gtk.csproj \
  -f net10.0-gtk -p:TreatWarningsAsErrors=false -p:RunAnalyzers=false \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeTizenTargetFrameworks=false \
  -p:IncludeWindowsTargetFrameworks=false

cd src/Controls/samples/Controls.Sample.Gtk/bin/Debug/net10.0-gtk/linux-arm64
xvfb-run -a -s "-screen 0 1280x1400x24" dotnet Controls.Sample.Gtk.dll
```

## Next steps

1. Exercise the menus at runtime.
2. Real clipboard on `Gdk.Clipboard`.
3. Batch `MauiCss` changes.
4. Register `MauiFont` in the CSS provider.
5. When upstream ships the GTK 4 workload, drop the probe commit and add
   `net10.0-gtk4` as a first-class TFM.
