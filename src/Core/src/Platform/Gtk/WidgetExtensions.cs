using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{
	public static class WidgetExtensions
	{
		public static void UpdateIsEnabled(this Widget platformView, bool isEnabled) =>
			platformView.Sensitive = isEnabled;

		public static void UpdateVisibility(this Widget platformView, Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Hidden:
					platformView.Visible = false;

					break;
				case Visibility.Visible:
					platformView.Visible = true;

					break;
				case Visibility.Collapsed:

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
			}
		}

		public static SizeRequest GetDesiredSize(
			this Widget? platformView,
			double widthConstraint,
			double heightConstraint)
		{
			if (platformView == null)
				return Graphics.Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			// GTK4 sustituye GetPreferredWidth/Height* por Measure().
			platformView.Measure(Orientation.Horizontal, heightConstrained ? (int)heightConstraint : -1,
				out var minimumWidth, out var naturalWidth, out _, out _);
			platformView.Measure(Orientation.Vertical, widthConstrained ? (int)widthConstraint : -1,
				out var minimumHeight, out var naturalHeight, out _, out _);

			if (platformView.WidthRequest > minimumWidth)
				minimumWidth = platformView.WidthRequest;

			if (platformView.HeightRequest > minimumHeight)
				minimumHeight = platformView.HeightRequest;

			return new SizeRequest(new Size(minimumWidth, minimumHeight), new Size(naturalWidth, naturalHeight));
		}
		public static void Arrange(this Widget? platformView, Rect rect, bool queue = true)
		{
			if (platformView == null)
				return;

			if (rect.IsEmpty)
				return;

			if (rect != platformView.Allocation.ToRect())
			{
				platformView.SizeAllocate(rect.ToNative());

				if (queue)
					platformView.QueueAllocate();
			}
		}

		public static void InvalidateMeasure(this Widget platformView, IView view)
		{
			platformView.QueueAllocate();
		}

		public static int Request(double viewSize) => viewSize >= 0 ? (int)viewSize : -1;

		public static void UpdateWidth(this Widget platformView, IView view)
		{
			var widthRequest = Request(view.Width);

			if (widthRequest != -1 && widthRequest != platformView.WidthRequest && widthRequest != platformView.AllocatedWidth)
			{
				platformView.WidthRequest = widthRequest;
				platformView.QueueResize();
			}
		}

		public static void UpdateHeight(this Widget platformView, IView view)
		{
			var heightRequest = Request(view.Height);

			if (heightRequest != -1 && heightRequest != platformView.HeightRequest && heightRequest != platformView.AllocatedHeight)
			{
				platformView.HeightRequest = heightRequest;
				platformView.QueueResize();
			}
		}

		public static void UpdateFont(this Widget platformView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;

			var fontFamily = fontManager.GetFontFamily(font)?.Family;
			var bold = font.Weight >= FontWeight.Bold;
			var italic = font.Slant is FontSlant.Italic or FontSlant.Oblique;

			MauiCss.SetFont(platformView, fontFamily, font.Size, bold, italic);
		}

		public static void ReplaceChild(this Gtk.Widget cont, Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			if (oldWidget.Parent != cont)
				return;

			switch (cont)
			{
				case IGtkContainer container:
					container.ReplaceChild(oldWidget, newWidget);

					break;
				case Gtk.Notebook notebook:
					{
						var pos = notebook.PageNum(oldWidget);
						var label = notebook.GetTabLabel(oldWidget);

						notebook.RemovePage(pos);
						notebook.InsertPage(newWidget, label, pos);

						break;
					}
				case Gtk.Paned paned:
					{
						var pos = paned.Position;
						var resizeStart = paned.ResizeStartChild;
						var shrinkStart = paned.ShrinkStartChild;
						var resizeEnd = paned.ResizeEndChild;
						var shrinkEnd = paned.ShrinkEndChild;

						if (paned.StartChild == oldWidget)
						{
							paned.StartChild = null;
							paned.StartChild = newWidget;
							paned.ResizeStartChild = resizeStart;
							paned.ShrinkStartChild = shrinkStart;
						}
						else
						{
							paned.EndChild = null;
							paned.EndChild = newWidget;
							paned.ResizeEndChild = resizeEnd;
							paned.ShrinkEndChild = shrinkEnd;
						}

						paned.Position = pos;

						break;
					}
				case Gtk.Container container:
					container.Remove(oldWidget);
					container.Add(newWidget);

					break;
			}
		}

		[MissingMapper]
		public static void UpdateMinimumHeight(this Widget platformView, IView view)
		{
			UpdateHeight(platformView, view);
		}

		[MissingMapper]
		public static void UpdateMinimumWidth(this Widget platformView, IView view)
		{
			UpdateWidth(platformView, view);
		}

		[MissingMapper]
		public static void UpdateMaximumHeight(this Widget platformView, IView view) { }

		[MissingMapper]
		public static void UpdateMaximumWidth(this Widget platformView, IView view) { }
	}
}