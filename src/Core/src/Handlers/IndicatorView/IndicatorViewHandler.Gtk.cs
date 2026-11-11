using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, IndicatorView>
	{
		protected override IndicatorView CreatePlatformView() => new();

		static IndicatorView? Platform(IIndicatorViewHandler handler) => handler.PlatformView as IndicatorView;

		static void Redraw(IIndicatorViewHandler handler) => Platform(handler)?.QueueDraw();

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
			{
				platformView.Count = indicator.Count;
				platformView.IndicatorSize = indicator.IndicatorSize;
				platformView.MaximumVisible = indicator.MaximumVisible;
				platformView.HideSingle = indicator.HideSingle;
				platformView.Position = indicator.Position;
				platformView.QueueResize();
				Redraw(handler);
			}
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
			{
				platformView.Position = indicator.Position;
				Redraw(handler);
			}
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
				platformView.HideSingle = indicator.HideSingle;

			Redraw(handler);
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
				platformView.MaximumVisible = indicator.MaximumVisible;

			Redraw(handler);
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
			{
				platformView.IndicatorSize = indicator.IndicatorSize;
				platformView.QueueResize();
			}

			Redraw(handler);
		}

		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
				platformView.IndicatorColor = indicator.IndicatorColor.ToColor() ?? Graphics.Colors.LightGray;

			Redraw(handler);
		}

		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
				platformView.SelectedIndicatorColor = indicator.SelectedIndicatorColor.ToColor() ?? Graphics.Colors.Black;

			Redraw(handler);
		}

		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			if (Platform(handler) is { } platformView)
			{
				var shape = indicator.IndicatorsShape;
				platformView.IsCircle = shape is null || shape.GetType().Name.Contains("Ellipse", StringComparison.OrdinalIgnoreCase);
			}

			Redraw(handler);
		}
	}
}
