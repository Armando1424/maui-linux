using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Benchmarks;

[MemoryDiagnoser]
public class LayoutBenchmarks
{
	LayoutHandler _handler = null!;
	IView _layout = null!;

	[GlobalSetup]
	public void Setup()
	{
		_layout = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "uno" },
				new Button { Text = "dos" },
				new Entry { Placeholder = "tres" },
				new Label { Text = "cuatro" },
			},
		};

		_handler = new LayoutHandler();
		_handler.SetMauiContext(BenchmarkContext.Context);
		_handler.SetVirtualView(_layout);
	}

	[GlobalCleanup]
	public void Cleanup() => ((IElementHandler)_handler).DisconnectHandler();

	[Benchmark]
	public Graphics.Size Measure() => _handler.GetDesiredSize(800, 600);

	[Benchmark]
	public void MeasureAndArrange()
	{
		_ = _handler.GetDesiredSize(800, 600);
		_handler.PlatformArrange(new Graphics.Rect(0, 0, 800, 600));
	}
}
