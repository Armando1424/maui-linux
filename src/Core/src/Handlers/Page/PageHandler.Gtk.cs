using System;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class PageHandler : ContentViewHandler
	{

		public static void MapTitle(IPageHandler handler, IContentView page)
		{
			if (handler.MauiContext is not { } context)
				return;

			// Inside a NavigationPage the title belongs to the toolbar; the
			// window title is the fallback for pages shown on their own.
			if (context.Services.GetService<Gtk.Window>() is { } window && page is ITitledElement titled)
				window.Title = titled.Title ?? window.Title;
		}

	}

}