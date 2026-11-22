using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation
	{
		static bool PlatformKeepScreenOn
		{
			get => throw ExceptionUtils.NotSupportedOrImplementedException;
			set => throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		protected override DisplayInfo GetMainDisplayInfo()
		{
			var mainMonitor = PrimaryMonitor;
			var geometry = mainMonitor.Geometry;
			var orientation = geometry.Height > geometry.Width ? DisplayOrientation.Portrait : DisplayOrientation.Landscape;
			var rate = mainMonitor.RefreshRate / 1000; // The value is in milli-Hertz, so a refresh rate of 60Hz is returned as 60000.

			return new DisplayInfo(geometry.Width, geometry.Height, 96.0, orientation, DisplayRotation.Unknown, rate);
		}

		protected override void StartScreenMetricsListeners()
		{
			// GTK4: GdkScreen ya no existe; sin evento de tamaño de pantalla.
			// TODO(GTK4): Gdk.Display.MonitorsChanged
		}

		void DefaultScreenOnSizeChanged(object sender, EventArgs e)
		{
			OnMainDisplayInfoChanged();
		}

		protected override void StopScreenMetricsListeners()
		{
			// GTK4: GdkScreen ya no existe; sin evento de tamaño de pantalla.
			// TODO(GTK4): Gdk.Display.MonitorsChanged
		}


		/// <summary>
		/// A Gdk.Visual (eliminado en GTK4) describes a particular video hardware display format.
		/// It includes information about the number of bits used for each color, the way the bits are translated into an RGB value for display,
		/// and the way the bits are stored in memory. For example, a piece of display hardware might support 24-bit color, 16-bit color, or 8-bit color;
		/// meaning 24/16/8-bit pixel sizes. For a given pixel size, pixels can be in different formats;
		/// for example the “red” element of an RGB pixel may be in the top 8 bits of the pixel, or may be in the lower 4 bits.
		/// There are several standard visuals.
		/// The visual returned by el visual del sistema is the system’s default visual,
		/// and the visual returned el visual RGBA should be used for creating windows with an alpha channel.
		///
		/// Get the system’s default visual for screen .
		/// This is the visual for the root window of the display.
		/// The return value should not be freed.
		/// </summary>
		// GTK4: GdkVisual ya no existe; el formato de pixel lo decide el compositor.

		public static double DefaultResolution => 96.0;

		/// <summary>
		/// GdkDisplay objects purpose are two fold:
		///	to manage and provide information about input devices (pointers and keyboards)
		///	to manage and provide information about the available Gdk.Displays.
		///	<see cref="Gdk.Display"/>'s are the GDK representation of an X Display, which can be described as a workstation consisting of a keyboard,
		/// a pointing device (such as a mouse) and one or more screens.
		/// It is used to open and keep track of various Gdk.Display objects currently instantiated by the application.
		/// It is also used to access the keyboard(s) and mouse pointer(s) of the display.
		/// </summary>
		public static Gdk.Display DefaultDisplay => Gdk.Display.Default;

		public static Gdk.Monitor CurrentMonitor => GetMonitors().FirstOrDefault()!; // TODO(GTK4): gdk_display_get_monitor_at_surface

		// GTK4 quitó gdk_monitor_is_primary: el primero del display hace de primario.
		public static Gdk.Monitor PrimaryMonitor => GetMonitors().FirstOrDefault()!;

		public static int CurrentScaleFactor = CurrentMonitor.ScaleFactor;

		public static IEnumerable<Gdk.Monitor> GetMonitors()
		{
			var monitors = DefaultDisplay.Monitors;
			for (uint i = 0; i < monitors.NItems; i++)
			{
				if (GLib.Object.GetObject(monitors.GetItem(i)) is Gdk.Monitor monitor)
					yield return monitor;
			}
		}

		protected override bool GetKeepScreenOn()
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}
	}
}