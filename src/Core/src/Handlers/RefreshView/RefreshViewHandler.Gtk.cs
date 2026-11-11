using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, RefreshView>
	{
		protected override RefreshView CreatePlatformView() => new();

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
		{
			if (handler.PlatformView is RefreshView platformView)
				platformView.IsRefreshing = refreshView.IsRefreshing;
		}

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
		{
			if (handler.PlatformView is not RefreshView platformView || handler.MauiContext is not { } context)
				return;

			platformView.Content = refreshView.Content?.ToPlatform(context);
		}

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
		{
			if (handler.PlatformView is RefreshView platformView)
				platformView.Spinner.SetStyleColor(refreshView.RefreshColor.ToColor(), "spinner", "color");
		}

		public static void MapRefreshViewBackground(IRefreshViewHandler handler, IView view)
		{
			if (handler.PlatformView is RefreshView platformView)
				platformView.UpdateBackground(view);
		}
	}
}
