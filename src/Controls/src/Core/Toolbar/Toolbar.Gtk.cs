using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{

	public partial class Toolbar
	{

		MauiToolbar PlatformView => Handler?.PlatformView as MauiToolbar ?? throw new InvalidOperationException("Native View not set");

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarTextColor((IToolbarHandler)arg1, arg2);

		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2) =>
			MapBarBackground((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonTitle((IToolbarHandler)arg1, arg2);

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2) =>
			MapToolbarItems((IToolbarHandler)arg1, arg2);

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleView((IToolbarHandler)arg1, arg2);

		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2) =>
			MapIconColor((IToolbarHandler)arg1, arg2);

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2) =>
			MapTitleIcon((IToolbarHandler)arg1, arg2);

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapBackButtonVisible((IToolbarHandler)arg1, arg2);

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2) =>
			MapIsVisible((IToolbarHandler)arg1, arg2);

		public static void MapBarTextColor(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is { } platformView)
				platformView.BarTextColor = toolbar.BarTextColor;
		}

		public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateIsVisible(toolbar);
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView?.UpdateBackButtonVisibility(toolbar);
		}

		public static void MapTitleIcon(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is not { } platformView || handler.MauiContext is not { } context)
				return;

			platformView.TitleIcon = toolbar.TitleIcon?.ToPlatform(context) as Gtk.Image;
		}

		public static void MapTitleView(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is not { } platformView || handler.MauiContext is not { } context)
				return;

			platformView.TitleView = toolbar.TitleView?.ToPlatform(context);
		}

		public static void MapIconColor(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is { } platformView)
				platformView.IconColor = toolbar.IconColor;
		}

		public static void MapToolbarItems(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is not { } platformView)
				return;

			foreach (var child in platformView.ItemsBox.Children)
				platformView.ItemsBox.Remove(child);

			foreach (var item in toolbar.ToolbarItems)
			{
				var button = new Gtk.Button(item.Text ?? string.Empty)
				{
					Relief = Gtk.ReliefStyle.None,
				};

				button.Clicked += (_, _) =>
				{
					if (item.Command?.CanExecute(item.CommandParameter) == true)
						item.Command.Execute(item.CommandParameter);
				};

				platformView.ItemsBox.PackStart(button, false, false, 0);
				button.Show();
			}
		}

		public static void MapBackButtonTitle(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is { } platformView)
				platformView.BackButtonTitle = toolbar.BackButtonTitle;
		}

		public static void MapBarBackground(IToolbarHandler handler, Toolbar toolbar)
		{
			if (handler.PlatformView is { } platformView && toolbar.BarBackground is SolidColorBrush brush)
				platformView.BarBackgroundColor = brush.Color;
		}

	}

}