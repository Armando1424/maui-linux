using Gtk;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.Switch.html

	public partial class SwitchHandler : ViewHandler<ISwitch, Switch>
	{
		// 
		protected override Switch CreatePlatformView()
		{
			return new Switch();
		}

		protected override void ConnectHandler(Switch nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.StateSet += OnStateSet;
		}

		protected override void DisconnectHandler(Switch nativeView)
		{
			nativeView.StateSet -= OnStateSet;
			base.DisconnectHandler(nativeView);
		}

		void OnStateSet(object o, StateSetArgs args)
		{
			if (VirtualView is { } virtualView && virtualView.IsOn != PlatformView.Active)
				virtualView.IsOn = PlatformView.Active;
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler.PlatformView is { } platformView)
				platformView.SetStyleColor(view.TrackColor, "switch", "background-color");
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler.PlatformView is not { } nativeView)
				return;

			nativeView.SetColor(view.ThumbColor, "color", "slider");
		}
	}
}
