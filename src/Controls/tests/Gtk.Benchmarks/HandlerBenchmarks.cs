using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Benchmarks;

[MemoryDiagnoser]
public class HandlerBenchmarks
{
	Label _label = null!;
	LabelHandler _handler = null!;

	[GlobalSetup]
	public void Setup()
	{
		_label = new Label { Text = "inicio" };
		_handler = new LabelHandler();
		_handler.SetMauiContext(BenchmarkContext.Context);
		_handler.SetVirtualView(_label);
	}

	[GlobalCleanup]
	public void Cleanup() => ((IElementHandler)_handler).DisconnectHandler();

	[Benchmark]
	public void UpdateText() => _label.Text = "actualizado";
}
