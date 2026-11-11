using BenchmarkDotNet.Running;

namespace Microsoft.Maui.Controls.Gtk.Benchmarks;

public static class Program
{
	public static void Main(string[] args)
		=> BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
