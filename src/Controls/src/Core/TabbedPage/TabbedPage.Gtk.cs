using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		bool _syncingTabs;

		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is not TabbedView platformView)
				return;

			if (view.BarBackground is SolidColorBrush brush)
				platformView.UpdateBarColors(brush.Color, null);
		}

		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is TabbedView platformView)
				platformView.UpdateBarColors(view.BarBackgroundColor, null);
		}

		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is TabbedView platformView)
				platformView.UpdateBarColors(null, view.BarTextColor);
		}

		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is TabbedView platformView)
				platformView.UpdateTabColors(view.SelectedTabColor, view.UnselectedTabColor);
		}

		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is TabbedView platformView)
				platformView.UpdateTabColors(view.SelectedTabColor, view.UnselectedTabColor);
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{
			view.SyncTabs();
		}

		[MissingMapper("creating pages from an ItemTemplate is not implemented on GTK")]
		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{
		}

		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is not TabbedView platformView)
				return;

			if (view.SelectedItem is Page selectedPage)
				platformView.SetCurrent(selectedPage);
		}

		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{
			if (handler.PlatformView is TabbedView platformView && view.CurrentPage is { } current)
				platformView.SetCurrent(current);
		}

		void SyncTabs()
		{
			if (_syncingTabs || Handler?.PlatformView is not TabbedView platformView || Handler.MauiContext is not { } context)
				return;

			_syncingTabs = true;

			try
			{
				var pages = new List<(IView, global::Gtk.Widget, string?)>();

				foreach (var page in Children)
					pages.Add((page, page.ToPlatform(context), page.Title));

				platformView.SetPages(pages, CurrentPage);

				platformView.CurrentPageChanged -= OnPlatformPageChanged;
				platformView.CurrentPageChanged += OnPlatformPageChanged;
			}
			finally
			{
				_syncingTabs = false;
			}
		}

		void OnPlatformPageChanged(object? sender, int index)
		{
			if (_syncingTabs || sender is not TabbedView platformView)
				return;

			if (index >= 0 && index < platformView.Views.Count && platformView.Views[index] is Page page)
				CurrentPage = page;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is { })
				SyncTabs();
		}
	}
}
