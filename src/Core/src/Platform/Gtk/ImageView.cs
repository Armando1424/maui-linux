using System;
using System.Net.Mime;
using Cairo;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Rectangle = Gdk.Rectangle;

namespace Microsoft.Maui.Platform
{
	// https://docs.gtk.org/gtk3/class.Image.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
	// or Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView

	public class ImageView : Gtk.DrawingArea
	{
		public ImageView()
		{
			CanFocus = true;
			AddEvents((int)Gdk.EventMask.AllEventsMask);
		}

		Gdk.Pixbuf? Pixbuf;

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set
			{
				Pixbuf = value;
				QueueResize();
			}
		}

		Aspect _aspect;

		public Aspect Aspect
		{
			get => _aspect;
			set
			{
				_aspect = value;
			}
		}

		public static Size Measure(Aspect aspect, Size desiredSize, double? widthConstraint, double? heightConstraint)
		{
			double desiredAspect = desiredSize.Width / desiredSize.Height;
			if (widthConstraint is not { } && heightConstraint is { })
				widthConstraint = desiredSize.Width * heightConstraint / desiredSize.Height;
			if (widthConstraint is { } && heightConstraint is not { })
				heightConstraint = desiredSize.Height * widthConstraint / desiredSize.Width;
			if (widthConstraint is not { } && heightConstraint is not { })
			{
				widthConstraint = desiredSize.Width;
				heightConstraint = desiredSize.Height;
			}

			double constraintAspect = widthConstraint!.Value / heightConstraint!.Value;

			double desiredWidth = desiredSize.Width;
			double desiredHeight = desiredSize.Height;

			if (desiredWidth == 0 || desiredHeight == 0)
				return new SizeRequest(new Size(0, 0));

			double width = desiredWidth;
			double height = desiredHeight;
			if (constraintAspect > desiredAspect)
			{
				// constraint area is proportionally wider than image
				switch (aspect)
				{
					case Aspect.AspectFit:
						width = desiredWidth * (height / desiredHeight);
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.AspectFill:
						height = heightConstraint!.Value;
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.Fill:
						width = Math.Min(desiredWidth, widthConstraint!.Value);
						height = desiredHeight * (width / desiredWidth);
						break;
				}
			}
			else if (constraintAspect < desiredAspect)
			{
				// constraint area is proportionally taller than image
				switch (aspect)
				{
					case Aspect.AspectFit:
						width = Math.Min(desiredWidth, widthConstraint!.Value);
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.AspectFill:
						width = widthConstraint!.Value;
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.Fill:
						height = Math.Min(desiredHeight, heightConstraint!.Value);
						width = desiredWidth * (height / desiredHeight);
						break;
				}
			}
			else
			{
				// constraint area is same aspect as image
				width = Math.Min(desiredWidth, widthConstraint!.Value);
				height = desiredHeight * (width / desiredWidth);
			}

			return new Size(width, height);
		}

		protected override bool OnDrawn(Context cr)
		{
			var allocation = Allocation;
			var stc = this.StyleContext;
			// stc.RenderBackground(cr, 0, 0, a.Width, a.Height);

			if (Image is not { } image)
				return true;

			// HACK: Gtk sends sometimes a draw event while the widget reallocates.
			//       In that case we would draw in the wrong area, which may lead to artifacts
			//       if no other widget updates it. Alternative: we could clip the
			//       allocation bounds, but this may have other issues.
			if (allocation.Width == 1 && allocation.Height == 1 && allocation.X == -1 && allocation.Y == -1) // the allocation coordinates on reallocation
				return base.OnDrawn(cr);


			var imgSize = Measure(Aspect, new Size(image.Width, image.Height), allocation.Width, allocation.Height);
			var x = (allocation.Width - imgSize.Width) / 2;
			var y = (allocation.Height - imgSize.Height) / 2;

			cr.DrawPixbuf(image, x, y, imgSize.Width, imgSize.Height);

			return false;
		}

		protected override void OnMeasure(Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimumBaseline = -1;
			naturalBaseline = -1;

			if (Image is { })
			{
				var imgSize = new Size(Image.Width, Image.Height);

				if (orientation == Orientation.Horizontal)
				{
					var size = Measure(Aspect, imgSize, forSize < 0 ? 0 : forSize, default);
					minimum = (int)size.Width;
					natural = Math.Max(Image.Width, minimum);
				}
				else
				{
					var size = Measure(Aspect, imgSize, default, forSize < 0 ? 0 : forSize);
					minimum = (int)size.Height;
					natural = Math.Max(Image.Height, minimum);
				}
				return;
			}

			base.OnMeasure(orientation, forSize, out minimum, out natural, out minimumBaseline, out naturalBaseline);
		}

		protected override void OnRealized()
		{
			base.OnRealized();
		}


		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
		}


	}
}