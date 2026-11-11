using System;
using Cairo;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Dots/squares indicator drawn with Cairo. GTK has no native indicator
	/// widget, so it paints the visible indicators centered in its allocation.
	/// </summary>
	public class IndicatorView : Gtk.DrawingArea
	{
		public int Count { get; set; }

		public int Position { get; set; }

		public int MaximumVisible { get; set; } = int.MaxValue;

		public bool HideSingle { get; set; }

		public double IndicatorSize { get; set; } = 6;

		public Graphics.Color IndicatorColor { get; set; } = Graphics.Colors.LightGray;

		public Graphics.Color SelectedIndicatorColor { get; set; } = Graphics.Colors.Black;

		public bool IsCircle { get; set; } = true;

		public IndicatorView()
		{
			Halign = Gtk.Align.Fill;
			Valign = Gtk.Align.Center;
		}

		protected override bool OnDrawn(Context cr)
		{
			base.OnDrawn(cr);

			if (Count <= 0 || (HideSingle && Count <= 1))
				return false;

			var size = IndicatorSize > 0 ? IndicatorSize : 6;
			var spacing = size / 2;
			var visible = Math.Min(Count, MaximumVisible <= 0 ? Count : MaximumVisible);
			var totalWidth = visible * size + (visible - 1) * spacing;

			var startX = (Allocation.Width - totalWidth) / 2;
			var y = (Allocation.Height - size) / 2;

			for (var i = 0; i < visible; i++)
			{
				var index = FirstVisibleIndex(visible) + i;
				var x = startX + i * (size + spacing);
				var color = index == Position ? SelectedIndicatorColor : IndicatorColor;

				cr.SetSourceColor(color.ToCairoColor());

				if (!IsCircle)
					cr.Rectangle(x, y, size, size);
				else
					cr.Arc(x + size / 2, y + size / 2, size / 2, 0, 2 * Math.PI);

				cr.Fill();
			}

			return false;
		}

		int FirstVisibleIndex(int visible)
		{
			if (visible >= Count)
				return 0;

			var half = visible / 2;
			return Math.Clamp(Position - half, 0, Count - visible);
		}
	}
}
