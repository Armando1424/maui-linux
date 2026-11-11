using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// CSS helpers for the GTK widgets. GTK3 has no border/radius API, so the
	/// mappings that come from MAUI's button strokes are translated to CSS.
	/// </summary>
	public static class GtkStyleExtensions
	{
		public static void UpdateBorderColor(this Gtk.Widget widget, Color? color)
		{
			if (color is null)
				return;

			widget.SetStyleColor(color, widget.CssMainNode(), "border-color");
			widget.SetStyleValue("solid", "border-style");
		}

		public static void UpdateBorderWidth(this Gtk.Widget widget, double width)
		{
			if (width < 0)
				return;

			widget.SetStyleValue($"{width:0.##}px", "border-width");
			widget.SetStyleValue("solid", "border-style");
		}

		public static void UpdateCornerRadius(this Gtk.Widget widget, CornerRadius radius)
		{
			if (radius.TopLeft < 0 || radius.TopRight < 0 || radius.BottomRight < 0 || radius.BottomLeft < 0)
				return;

			widget.SetStyleValue(
				$"{radius.TopLeft:0.##}px {radius.TopRight:0.##}px {radius.BottomRight:0.##}px {radius.BottomLeft:0.##}px",
				"border-radius");
		}
	}
}
