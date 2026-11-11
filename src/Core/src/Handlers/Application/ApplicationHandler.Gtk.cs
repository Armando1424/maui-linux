using System;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Gtk.Application>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			Gtk.Application.Quit();
		}

		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.CreatePlatformWindow(application, args as OpenWindowRequest);
		}

		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
				(window.Handler?.PlatformView as Gtk.Window)?.Close();
		}
	}
}