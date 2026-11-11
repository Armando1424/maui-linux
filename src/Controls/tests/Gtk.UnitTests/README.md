# GTK handler tests

Unit tests for the GTK handlers of `Microsoft.Maui.Controls`. The project
targets `net10.0-gtk` and runs on the GtkSharp workload.

## Running

```sh
# Headless (CI and servers)
xvfb-run -a dotnet test src/Controls/tests/Gtk.UnitTests -c Release

# With a desktop session
dotnet test src/Controls/tests/Gtk.UnitTests -c Release
```

When no display is available the GTK tests are **skipped** (not failed).
The GTK infrastructure is the same as GtkSharp's: `[GtkFact]` reports a skip,
the `Gtk` collection serializes the tests and `GtkFixture` initializes GTK
once per process.

## The pattern

```csharp
var button = new Button { Text = "Save" };

var handler = new ButtonHandler();
handler.SetMauiContext(HandlerTestContext.Context);
handler.SetVirtualView(button);

Assert.Equal("Save", handler.PlatformView!.Label);
```

`HandlerTestContext` builds one `MauiApp` with the core services (fonts,
dispatcher, handler registry). Use `HandlerTestContext.Pump()` after actions
that go through `Gtk.Application.Invoke` (for example, a button click) so the
posted callback runs.

## Covered handlers (50 tests)

| Handler | Tests |
|:--|--:|
| `ButtonHandler` | 7 (text, enabled, click, command, parameter, CanExecute, update) |
| `EntryHandler` | 8 (text, placeholder, max length, password, read-only, empty, native change, update) |
| `LabelHandler` | 5 (text, alignment, line break, update, decorations) |
| `CheckBoxHandler` | 4 (checked, default, native toggle, update) |
| `SwitchHandler` | 4 (on, default, update, thumb color) |
| `SliderHandler` | 5 (range, value, native change, update, default) |
| `ProgressBarHandler` | 3 (progress, tooltip, update) |
| `ActivityIndicatorHandler` | 3 (running, stopped, update) |
| `LayoutHandler` | 5 (add, multiple, remove, clear, insert) |
| `NavigationView` | 6 (root, push, push replaces, pop, pop disconnects, 25 cycles) |

The layout tests found a real bug: `LayoutView` removed the native widget but
kept the entry in its `_children` list, so the next draw touched a freed
widget and crashed the process. `LayoutView.Remove(IView, Widget)` fixes it and
`LayoutHandler.Remove` now uses it.

## Navigation

`NavigationHandlerTests` drives `NavigationView` directly instead of
`NavigationPage.PushAsync`, so the GTK stack management is deterministic. The
navigation page only gets a `TestElementHandler` to expose the `MauiContext`.

These tests found that popped pages kept their handler (and native widget)
alive; `NavigationView` now disconnects every page that leaves the stack.

## Performance

Tests tagged `Category=Performance` live in `Performance/` and run separately:

```sh
xvfb-run -a dotnet test src/Controls/tests/Gtk.UnitTests \
  -c Release -p:_IncludeAndroid= --filter "Category=Performance"
```

They cover handler churn, label updates, measure/arrange and push/pop cycles.
Thresholds and the Pi baseline are in `docs/performance-gtk.md`. The 30 minute
soak lives in `eng/soak/gtk-soak.sh`.

## Known gaps

`[MissingMapper]` marks mappings that are intentionally not implemented on
GTK. There are 75 of them; the largest groups are:

| Handler | Missing mappings |
|:--|--:|
| `ViewHandler` | 11 |
| `EditorHandler` | 11 |
| `EntryHandler` | 7 |
| `SearchBarHandler` | 6 |
| `ButtonHandler` | 5 (image source, font, stroke color/thickness, corner radius) |
| `TimePickerHandler` / `RefreshViewHandler` | 4 each |
| `RadioButtonHandler` / `ImageButtonHandler` | 4 each |
| `SliderHandler` | 2 (track colors) |
| `SwitchHandler` | 1 (track color) |

`SwitchHandler` also only maps one way: changing the native switch does not
update `IsToggled`. Write a test before implementing it.

## Adding a handler test

1. Add a `*HandlerTests.cs` file in `HandlerTests/`.
2. Use `[Collection(GtkCollection.Name)]` and `[GtkFact]`.
3. Build the handler through `SetMauiContext` + `SetVirtualView`.
4. Assert the GTK widget state, and the native → virtual direction when the
   handler subscribes to a GTK signal.
5. If a mapping cannot be implemented, add `[MissingMapper]` and document it
   here; do not leave a silent no-op.
