using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;

public sealed class TestApplication : Application
{
}

/// <summary>
/// Shared MauiContext and helpers for the handler tests. Building a MauiApp
/// registers the core services the handlers need (fonts, dispatcher, ...).
/// </summary>
public static class HandlerTestContext
{
    private static readonly MauiApp App = MauiApp.CreateBuilder()
        .UseMauiApp<TestApplication>()
        .Build();

    public static IMauiContext Context { get; } = new MauiContext(App.Services);

    /// <summary>
    /// Runs the GTK main context once so callbacks posted with
    /// Gtk.Application.Invoke (used by the GTK dispatcher) are executed.
    /// </summary>
    public static void Pump()
    {
        while (global::Gtk.Application.EventsPending())
            global::Gtk.Application.RunIteration(false);
    }
}
