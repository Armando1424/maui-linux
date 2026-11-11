using System;
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
    /// A context for handlers that need a platform window (toolbar mapping goes
    /// through <c>Gtk.Window</c>). The window is registered only for the scope of
    /// the returned context, like <c>MakeWindowScope</c> does in a real app.
    /// </summary>
    public static IMauiContext CreateWithWindow(global::Gtk.Window window)
        => new MauiContext(new WindowServiceProvider(App.Services, window));

    sealed class WindowServiceProvider : IServiceProvider
    {
        readonly IServiceProvider _inner;
        readonly global::Gtk.Window _window;

        public WindowServiceProvider(IServiceProvider inner, global::Gtk.Window window)
        {
            _inner = inner;
            _window = window;
        }

        public object? GetService(Type serviceType)
            => serviceType == typeof(global::Gtk.Window) ? _window : _inner.GetService(serviceType);
    }

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
