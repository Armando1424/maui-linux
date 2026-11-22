#define TRACE_ALLOCATION_
#define USE_ADJUSTSIZEREQUEST_
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Point = Microsoft.Maui.Graphics.Point;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

#pragma warning disable CS0162 // Unreachable code detected

namespace Microsoft.Maui.Platform
{
	// refactored from: https://github.com/mono/xwt/blob/501f6b529fca632655295169094f637627c74c47/Xwt.Gtk/Xwt.GtkBackend/BoxBackend.cs

	// see: https://github.com/GNOME/gtk/blob/gtk-3-22/gtk/gtkcontainer.c

	public class LayoutView : Container, IGtkContainer, ICrossPlatformLayoutBacking
	{
		protected override void OnSnapshot(Gtk.Snapshot snapshot)
		{
			// GTK4: los hijos los dibuja GTK; aquí solo el fondo del layout.
			base.OnSnapshot(snapshot);
		}

		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

		List<(IView view, Widget widget)> _children = new();

		public LayoutView()
		{
		}

		public void ReplaceChild(Widget oldWidget, Widget newWidget)
		{
			var index = _children.FindIndex(c => c.widget == oldWidget);

			if (index == -1)
				return;

			var view = _children[index].view;

			Remove(oldWidget);
			Add(newWidget);

			if (view != null)
			{
				_children[index] = (view, newWidget);
			}
		}

		public void ClearChildren()
		{
			ClearMeasured();

			foreach (var c in _children.ToArray())
			{
				Remove(c.view, c.widget);
			}

			_children.Clear();
		}

		public void Remove(IView view, Widget widget)
		{
			var index = _children.FindIndex(c => c.widget == widget);
			if (index != -1)
				_children.RemoveAt(index);

			if (widget.Parent == this)
				Remove(widget);

			ClearMeasured();
			QueueResize();
		}

		public void SetZIndex(IView view, int index)
		{
			var current = _children.FindIndex(c => ReferenceEquals(c.view, view));
			if (current < 0)
				return;

			index = Math.Clamp(index, 0, _children.Count - 1);
			if (current == index)
				return;

			var item = _children[current];
			_children.RemoveAt(current);
			_children.Insert(index, item);

			// GTK draws and lays out children in container order.
			foreach (var child in _children)
				Remove(child.widget);
			foreach (var child in _children)
				Add(child.widget);

			QueueDraw();
		}

		public void Add(IView view, Widget widget)
		{
			var index = _children.FindIndex(c => c.widget == widget);

			if (index != -1)
				return;

			_children.Add((view, widget));

			Add(widget);
		}

		public void Insert(IView view, Widget widget, int index)
		{
			_children.Insert(index, (view, widget));
			Add(widget);
		}

		public void Update(IView view, Widget widget, int index)
		{
			var replace = _children[index];
			_children[index] = (view, widget);
			Remove(replace.widget);
			Add(widget);
		}

		// GTK4 no tiene OnAdded/OnRemoved: el alta y la baja pasan por Add/Remove
		// del Container de compatibilidad.
		public override void Add(Widget widget)
		{
			base.Add(widget);
			ClearMeasured();
		}

		public override void Remove(Widget widget)
		{
			base.Remove(widget);
			ClearMeasured();
			QueueResize();
		}

		protected void AllocateChildren(Rectangle allocation)
		{
			foreach (var cr in _children.ToArray())
			{
				var w = cr.widget;
				var v = cr.view;
				var r = v.Frame;

				if (r.IsEmpty)
					continue;

				var cAlloc = new Gdk.Rectangle((int)(allocation.X + r.X), (int)(allocation.Y + r.Y), (int)r.Width, (int)r.Height);

				// if (cAlloc != w.Allocation) // it's allways needed to implicit arrange children:
					w.SizeAllocate(cAlloc);
			}
		}

		protected void ArrangeAllocation(Rectangle allocation)
		{
			if (CrossPlatformLayout is not { } virtualView)
			{
				return;
			}

			virtualView.CrossPlatformArrange(allocation);
		}

		protected bool RestrictToMeasuredAllocation { get; set; } = false;

		protected bool RestrictToMeasuredArrange { get; set; } = false;

		protected bool IsReallocating;

		protected bool IsSizeAllocating;

		protected Size? MeasuredSizeH { get; set; }

		protected Size? MeasuredSizeV { get; set; }

		protected Rectangle LastAllocation { get; set; }

		protected Rectangle? CurrentAllocation { get; set; }

				double? RequestedWidth { get; set; }

		double? RequestedHeight { get; set; }

		protected void ClearMeasured(bool clearCache = true)
		{
			if (clearCache)
			{
				MeasuredSizeH = null;
				MeasuredSizeV = null;
			}

#if !USE_ADJUSTSIZEREQUEST
			RequestedWidth = null;
			RequestedHeight = null;

#endif
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			if (IsSizeAllocating)
				return;

			if (CrossPlatformLayout is not { } crossPlatformLayout)
			{
				base.OnSizeAllocated(allocation);

				return;
			}

			var clearCache = true;

			try
			{
				IsReallocating = true;

				var mAllocation = allocation.ToRect();
				CurrentAllocation = mAllocation;

				clearCache = LastAllocation.IsEmpty || mAllocation.IsEmpty || LastAllocation != mAllocation;
				ClearMeasured(clearCache);

				if (RestrictToMeasuredAllocation)
				{
					var mesuredAllocation = Measure(allocation.Width, allocation.Height);
					mAllocation.Size = mesuredAllocation;
					CurrentAllocation = mAllocation;
				}

				ArrangeAllocation(new Rectangle(Point.Zero, mAllocation.Size));
				AllocateChildren(mAllocation);

				if (crossPlatformLayout is IView virualView && virualView.Frame != mAllocation)
				{
					// IsSizeAllocating = true;

					// Arrange(mAllocation);
				}

				// No se llama a base.OnSizeAllocated porque el Container de compat
				// re-colocaría los hijos en (0,0) con su tamaño natural, pisando el
				// arrange de MAUI. Solo se propaga el evento de tamaño.
				RaiseSizeAllocated(allocation);
				LastAllocation = mAllocation;
			}
			finally
			{
				IsReallocating = false;
				IsSizeAllocating = false;
				CurrentAllocation = null;
			}
		}

		protected override void OnUnrealized()
		{
			// force reallocation on next realization, since allocation may be lost
			IsReallocating = false;
			ClearMeasured();
			base.OnUnrealized();
		}

		protected override void OnRealized()
		{
			// force reallocation, if unrealized previously
			if (!IsReallocating)
			{
				try
				{
					LastAllocation = Allocation.ToRect();
					CurrentAllocation = LastAllocation;
					Measure(Allocation.Width, Allocation.Height);
				}
				catch
				{
					CurrentAllocation = null;
					IsReallocating = false;
				}
			}

			base.OnRealized();
		}

#if TRACE_ALLOCATION
		int _measureCount = 0;
#endif

		public Size Measure(double widthConstraint, double heightConstraint, SizeRequestMode mode = SizeRequestMode.ConstantSize)
		{
			if (CrossPlatformLayout is not { } virtualView)
			{
				return Size.Zero;
			}

			var measured = virtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);

#if TRACE_ALLOCATION
			_measureCount++;
#endif

			return measured;
		}

#if !USE_ADJUSTSIZEREQUEST
		// GTK4 sustituye los cuatro virtuales de GTK3 (preferred width/height y
		// width/height-for-*) por uno solo: OnMeasure(orientación, forSize).
		protected override void OnMeasure(Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimumBaseline = -1;
			naturalBaseline = -1;

			var width = double.PositiveInfinity;
			var height = double.PositiveInfinity;

			if (forSize >= 0)
			{
				if (orientation == Orientation.Horizontal)
					height = forSize;
				else
					width = forSize;
			}

			var size = Measure(width, height);

			if (orientation == Orientation.Horizontal)
			{
				var w = size.Width < WidthRequest ? WidthRequest : size.Width;
				minimum = natural = (int)System.Math.Ceiling(w);
			}
			else
			{
				var h = size.Height < HeightRequest ? HeightRequest : size.Height;
				minimum = natural = (int)System.Math.Ceiling(h);
			}
		}
#endif

#if USE_ADJUSTSIZEREQUEST
		// see: https://github.com/GNOME/gtk/blob/gtk-3-22/gtk/gtksizerequest.c#L362

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
		{
			base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

			if (IsSizeAllocating)
			{
				return;
			}

			if (CrossPlatformLayout is not { } virtualView)
				return;

			double allocation = orientation == Orientation.Vertical ? AllocatedWidth : AllocatedHeight;

			double hConstraint = IsReallocating ? AllocatedHeight : double.PositiveInfinity;
			double wConstraint = IsReallocating ? AllocatedWidth : 0;

			var requestMode = RequestMode;

			if (requestMode != SizeRequestMode.HeightForWidth)
			{
				;
			}

			if (orientation == Orientation.Horizontal)
			{
				// constraint = constraint == 0 && MeasuredSizeV is { } size ? size.Height : constraint;
				if (minimumSize > 0)
					wConstraint = minimumSize;
				var size = new Size(wConstraint, hConstraint);
				var measuredMinimum = Measure(size.Width, size.Height);
				MeasuredSizeH = measuredMinimum;
				minimumSize = (int)measuredMinimum.Width;
				naturalSize = (int)minimumSize;
			}

			if (orientation == Orientation.Vertical)
			{
				// constraint = constraint == 0 && MeasuredSizeH is { } hsize ? hsize.Width : constraint;
				if (minimumSize > 0)
					hConstraint = minimumSize;
				var size = new Size(wConstraint, hConstraint);
				var measuredMinimum = Measure(size.Width, size.Height);
				MeasuredSizeV = measuredMinimum;
				minimumSize = (int)measuredMinimum.Height;
				naturalSize = (int)minimumSize;
			}
		}
#endif

		public void Arrange(Rectangle rect)
		{
			if (rect.IsEmpty)
				return;

			if (rect == Allocation.ToRect())
				return;

			if (IsSizeAllocating)
			{
				SizeAllocate(rect.ToNative());

				return;
			}

			var size = rect.Size;

			if (RestrictToMeasuredArrange)
			{
				var measuredArrange = Measure(rect.Width, rect.Height);
				size = measuredArrange;
			}

			var alloc = new Rectangle(rect.Location, size);
			SizeAllocate(alloc.ToNative());
			QueueAllocate();
		}

		public Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (CrossPlatformLayout is not IView virtualView)
				return new Size(widthConstraint, heightConstraint);

			double? explicitWidth = (virtualView.Width >= 0) ? virtualView.Width : null;
			double? explicitHeight = (virtualView.Height >= 0) ? virtualView.Height : null;

			var measuredSize = Measure(explicitWidth ?? widthConstraint, explicitHeight ?? heightConstraint);

			// apply width and height constraints if necessary
			// var desiredWidth = Math.Min(measuredSize.Width, widthConstraint);
			// var desiredHeight = Math.Min(measuredSize.Height, heightConstraint);

			// return new Size(desiredWidth, desiredHeight);
			return measuredSize;
		}

		protected int ToSize(double it) => double.IsPositiveInfinity(it) ? 0 : (int)it;
	}
}