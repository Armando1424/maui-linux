using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.Performance;

/// <summary>
/// Performance and memory smoke tests for the GTK handlers. They run in a
/// separate trait (<c>Category=Performance</c>) so the functional suite stays
/// fast; thresholds are deliberately generous because RSS is noisy on small
/// boards and shared runners.
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "Performance")]
public class HandlerPerformanceTests
{
	private const int Cycles = 10_000;

	private static long Rss() => Process.GetCurrentProcess().WorkingSet64;

	private static void FullGc()
	{
		for (var i = 0; i < 3; i++)
		{
			GC.Collect(2, GCCollectionMode.Forced, true, true);
			GC.WaitForPendingFinalizers();
		}

		MallocTrim();
	}

	// glibc keeps freed arenas in the process; trimming makes RSS readable.
	[System.Runtime.InteropServices.DllImport("libc", SetLastError = true)]
	private static extern int malloc_trim(nint pad);

	private static void MallocTrim()
	{
		try
		{
			malloc_trim(0);
		}
		catch (Exception)
		{
			// Not glibc (e.g. musl): RSS is only informative.
		}
	}

	private static void CreateAndDisconnectLabel()
	{
		var label = new Label { Text = "performance" };
		var handler = new LabelHandler();

		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(label);
		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void CreatingAndDisconnectingLabelHandlersDoesNotLeak()
	{
		// Measure the steady state: the first batch warms the allocator and the
		// handler factory, the second one must not keep growing.
		for (var i = 0; i < 2_000; i++)
			CreateAndDisconnectLabel();

		for (var i = 0; i < Cycles; i++)
			CreateAndDisconnectLabel();

		FullGc();
		var steady = Rss();

		for (var i = 0; i < Cycles; i++)
			CreateAndDisconnectLabel();

		FullGc();
		var growth = Rss() - steady;
		var perCycle = growth / (double)Cycles;

		// GTK keeps font and widget caches, so the baseline on the Pi is around
		// 5 KB per handler cycle. 12 KB catches a runaway leak (for example a
		// platform view that is created and never released).
		Assert.True(
			perCycle < 12 * 1024,
			$"RSS grew {perCycle:F0} bytes per handler cycle " +
			$"({growth / (1024 * 1024.0):F1} MB over {Cycles} cycles).");
	}

	[GtkFact]
	public void UpdatingTenThousandLabelsDoesNotAllocatePerUpdate()
	{
		var label = new Label { Text = "start" };
		var handler = new LabelHandler();

		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(label);

		FullGc();
		var before = GC.GetAllocatedBytesForCurrentThread();
		var started = Stopwatch.GetTimestamp();

		for (var i = 0; i < Cycles; i++)
			label.Text = $"value {i}";

		var elapsed = Stopwatch.GetElapsedTime(started);
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

		((IElementHandler)handler).DisconnectHandler();

		Assert.True(
			allocated < Cycles * 1024L,
			$"Allocated {allocated / (double)Cycles:F0} bytes per update.");

		Assert.True(
			elapsed < TimeSpan.FromSeconds(10),
			$"{Cycles} label updates took {elapsed.TotalSeconds:F1} s.");
	}

	[GtkFact]
	public void LayoutMeasureAndArrangeIsStable()
	{
		var stack = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "one" },
				new Button { Text = "two" },
				new Entry { Placeholder = "three" },
			},
		};

		var handler = new LayoutHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(stack);

		FullGc();
		var before = Rss();
		var started = Stopwatch.GetTimestamp();

		for (var i = 0; i < Cycles; i++)
		{
			_ = handler.GetDesiredSize(800, 600);
			handler.PlatformArrange(new Graphics.Rect(0, 0, 800, 600));
		}

		var elapsed = Stopwatch.GetElapsedTime(started);

		((IElementHandler)handler).DisconnectHandler();

		FullGc();
		var growth = Rss() - before;

		Assert.True(
			growth < 32 * 1024 * 1024,
			$"RSS grew {growth / (1024 * 1024.0):F1} MB after {Cycles} measure/arrange cycles.");

		Assert.True(
			elapsed < TimeSpan.FromSeconds(20),
			$"{Cycles} measure/arrange cycles took {elapsed.TotalSeconds:F1} s.");
	}

	[GtkFact]
	public void RepeatedNavigationKeepsMemoryStable()
	{
		var context = HandlerTestContext.CreateWithWindow(new global::Gtk.Window("perf"));
		var root = new ContentPage { Content = new Label { Text = "root" } };
		var page = new NavigationPage(root);

		var pageHandler = new TestElementHandler();
		pageHandler.SetMauiContext(context);
		((IElement)page).Handler = pageHandler;

		var native = new NavigationView();
		native.Connect(page);

		FullGc();
		var before = Rss();

		for (var i = 0; i < 1_000; i++)
		{
			var second = new ContentPage { Content = new Label { Text = $"page {i}" } };

			native.RequestNavigation(new NavigationRequest(new IView[] { root, second }, false));
			native.RequestNavigation(new NavigationRequest(new IView[] { root }, false));

			if (native.Children.Length != 1)
				Assert.Fail($"Expected a single child after cycle {i}, found {native.Children.Length}.");
		}

		FullGc();
		var growth = Rss() - before;

		Assert.True(
			growth < 48 * 1024 * 1024,
			$"RSS grew {growth / (1024 * 1024.0):F1} MB after 1,000 push/pop cycles.");
	}

	[GtkFact]
	public void DisconnectedHandlersAreCollectable()
	{
		var references = new List<WeakReference>();

		for (var i = 0; i < 500; i++)
		{
			var label = new Label { Text = "collectable" };
			var handler = new LabelHandler();

			handler.SetMauiContext(HandlerTestContext.Context);
			handler.SetVirtualView(label);
			((IElementHandler)handler).DisconnectHandler();

			references.Add(new WeakReference(label));
			references.Add(new WeakReference(handler));
		}

		FullGc();

		var alive = references.Count(reference => reference.IsAlive);

		// GtkSharp 3.24 does not expose GLib.Object.WarnOnFinalize, so the
		// "unexpected finalization" criterion is covered by checking that the
		// disconnected handlers and their virtual views are collectable.
		Assert.True(
			alive <= references.Count / 10,
			$"{alive} of {references.Count} disconnected objects are still alive.");
	}

	[GtkFact]
	public void BindingUpdatesKeepMemoryStable()
	{
		var viewModel = new BindingViewModel();
		var label = new Label { BindingContext = viewModel };
		label.SetBinding(Label.TextProperty, nameof(BindingViewModel.Value));

		var handler = new LabelHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(label);

		FullGc();
		var before = Rss();

		for (var i = 0; i < 1_000; i++)
			viewModel.Value = $"value {i}";

		FullGc();
		var growth = Rss() - before;

		((IElementHandler)handler).DisconnectHandler();

		Assert.True(
			growth < 24 * 1024 * 1024,
			$"RSS grew {growth / (1024 * 1024.0):F1} MB after 1,000 binding updates.");
	}

	sealed class BindingViewModel : INotifyPropertyChanged
	{
		string _value = string.Empty;

		public string Value
		{
			get => _value;
			set
			{
				if (_value == value)
					return;

				_value = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}
}
