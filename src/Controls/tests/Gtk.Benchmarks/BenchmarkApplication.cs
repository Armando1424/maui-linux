using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Gtk.Benchmarks;

public sealed class BenchmarkApplication : Microsoft.Maui.Controls.Application
{
}

/// <summary>
/// GTK must be initialized once and the handlers need a MauiContext with the
/// core services, the same way the unit tests build them.
/// </summary>
public static class BenchmarkContext
{
	static readonly Lazy<MauiApp> App = new(() =>
	{
		var args = Array.Empty<string>();

		if (!global::Gtk.Application.InitCheck("Microsoft.Maui.Controls.Gtk.Benchmarks", ref args))
			throw new InvalidOperationException("GTK could not be initialized. Run under xvfb-run.");

		return MauiApp.CreateBuilder()
			.UseMauiApp<BenchmarkApplication>()
			.Build();
	});

	public static IMauiContext Context => new MauiContext(App.Value.Services);
}
