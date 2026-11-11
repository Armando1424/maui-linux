using System;
using System.Threading;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;

/// <summary>
/// Detects whether a GTK display is available and initializes GTK once per test
/// process. When no display is available the GTK tests are skipped instead of
/// failing, so the suite can run on a headless machine without Xvfb.
/// </summary>
public static class GtkTestEnvironment
{
    private static readonly Lazy<ProbeResult> Result = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool IsAvailable => Result.Value.Available;

    public static string UnavailableReason => Result.Value.Reason
        ?? "GTK is not available in this process.";

    private static ProbeResult Probe()
    {
        try
        {
            var args = Array.Empty<string>();

            if (global::Gtk.Application.InitCheck("Microsoft.Maui.Controls.Gtk.UnitTests", ref args))
                return new ProbeResult(true, null);

            return new ProbeResult(
                false,
                "GTK could not open a display. Run the tests under a virtual display: " +
                "`xvfb-run -a dotnet test src/Controls/tests/Gtk.UnitTests`.");
        }
        catch (Exception ex) when (
            ex is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException)
        {
            return new ProbeResult(false, $"GTK native libraries could not be loaded: {ex.Message}");
        }
    }

    private readonly record struct ProbeResult(bool Available, string? Reason);
}
