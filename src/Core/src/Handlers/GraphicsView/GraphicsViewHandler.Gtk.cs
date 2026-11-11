using System;

namespace Microsoft.Maui.Handlers
{

	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView() => new();

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			if (handler.PlatformView is { } platformView)
				platformView.Direction = graphicsView.FlowDirection switch
				{
					FlowDirection.RightToLeft => Gtk.TextDirection.Rtl,
					FlowDirection.LeftToRight => Gtk.TextDirection.Ltr,
					_ => Gtk.TextDirection.None,
				};
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.QueueDraw();
		}
	}

}