using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{

	public class ContentView : Container, ICrossPlatformLayoutBacking
	{
		public ContentView() : base() { }

		// Era un Gtk.Bin: el hijo ocupa todo el espacio disponible.
		protected override bool ChildrenFillAllocation => true;

		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

		Widget? _content;

		public Widget? Content
		{
			get => _content;
			set
			{
				if (_content != null && value != null)
				{
					this.ReplaceChild(_content, value);
				}
				else if (value != null)
				{
					//PackStart(value, true, false, 0);
					Child = value;
					Child.Hexpand = true;
				Child.Vexpand = true;
				}

				_content = value;
			}
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}
	}

}