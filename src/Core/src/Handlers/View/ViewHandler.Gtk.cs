namespace Microsoft.Maui.Handlers
{

	public partial class ViewHandler
	{
		partial void DisconnectingHandler(global::Gtk.Widget platformView)
		{
			// Gtk# widgets are not collected while GTK still tracks them, so a
			// disconnected handler would leak one native widget per cycle. Free
			// it when it is no longer inside a container; parented widgets keep
			// living until their container releases them.
			if (platformView is { Parent: null } widget && widget is not global::Gtk.Window)
				widget.Dispose();
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			var platformView = handler.ToPlatform();
			platformView.Arrange(view.Frame);
		}

		[MissingMapper("GTK3 widgets cannot be translated without a transform container")]
		public static void MapTranslationX(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be translated without a transform container")]
		public static void MapTranslationY(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be scaled without a transform container")]
		public static void MapScale(IViewHandler handler, IView view)
		{
			//handler.PlatformView.UpdateScale(view.Scale);
		}

		[MissingMapper("GTK3 widgets cannot be scaled without a transform container")]
		public static void MapScaleX(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be scaled without a transform container")]
		public static void MapScaleY(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be rotated")]
		public static void MapRotation(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be rotated")]
		public static void MapRotationX(IViewHandler handler, IView view) { }

		[MissingMapper("GTK3 widgets cannot be rotated")]
		public static void MapRotationY(IViewHandler handler, IView view) { }

		[MissingMapper("there is no transform origin on GTK3 widgets")]
		public static void MapAnchorX(IViewHandler handler, IView view) { }

		[MissingMapper("there is no transform origin on GTK3 widgets")]
		public static void MapAnchorY(IViewHandler handler, IView view) { }

		public static void MapToolbar(IViewHandler handler, IView view)
		{
			if (view is IToolbarElement tb)
				MapToolbar(handler, tb);
		}

		internal static void MapToolbar(IElementHandler handler, IToolbarElement tb)
		{
			if (handler.MauiContext is not null)
			{
				var toolbarContainer = handler.MauiContext.GetToolBarContainer();
				if (toolbarContainer is not null)
				{
					toolbarContainer.SetToolbar(tb.Toolbar?.ToPlatform(handler.MauiContext) as MauiToolbar);
				}
			}
		}
	}

}