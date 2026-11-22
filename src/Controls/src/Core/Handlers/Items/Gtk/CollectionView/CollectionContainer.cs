#define USE_ADJUSTSIZEREQUEST_

using System.Collections.Generic;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Point = Microsoft.Maui.Graphics.Point;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Controls.Handlers.Items.Platform;

public partial class CollectionContainer : Container
{
	public CollectionContainer()
	{
	}

	public ItemAdaptor? Adaptor
	{
		get;
		set;
	}

	public ICollectionViewLayoutManager? LayoutManager
	{
		get;
		set;
	}

	protected override void OnSnapshot(Gtk.Snapshot snapshot)
	{
		// GTK4 dibuja los hijos; aquí solo el fondo.
		base.OnSnapshot(snapshot);
	}

	public bool IsReallocating { get; set; }

	public bool IsSizeAllocating { get; set; }

	public IView? VirtualView { get; set; }

	protected Rectangle? CurrentAllocation { get; set; }

	Rect LastAllocation { get; set; }

	const bool RestrictToMesuredAllocation = false;

	protected Size? MeasuredSizeH { get; set; }

	protected Size? MeasuredSizeV { get; set; }

	protected Size? MeasuredMinimum { get; set; }

	protected void ClearMeasured(bool clearCache = true)
	{
		MeasuredSizeH = null;
		MeasuredSizeV = null;
		MeasuredMinimum = null;
#if !USE_ADJUSTSIZEREQUEST
		RequestedWidth = null;
		RequestedHeight = null;

#endif
	}

	List<Widget> _children = new();
	List<Widget> _items = new();

	public void AddItem(Widget widget)
	{
		_items.Add(widget);
		widget.Parent = this;
	}

	public void RemoveItem(Widget widget)
	{
		widget.Unparent();
		_items.Remove(widget);
	}

	public override void Add(Widget widget)
	{
		if (!_children.Contains(widget))
			_children.Add(widget);

		base.Add(widget);
		ClearMeasured();
	}

	public override void Remove(Widget widget)
	{
		base.Remove(widget);
		_children.Remove(widget);
		ClearMeasured();
		QueueResize();
	}

	protected override void OnSizeAllocate(int width, int height, int baseline)
	{
		var allocation = new Gdk.Rectangle(0, 0, width, height);

		if (IsSizeAllocating)
			return;

		if (VirtualView is not { } virtualView)
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

			LastAllocation = mAllocation;

			if (RestrictToMesuredAllocation)
#pragma warning disable CS0162 // Unreachable code detected
			{
				var mesuredAllocation = Measure(allocation.Width, allocation.Height);
				mAllocation.Size = mesuredAllocation;
			}
#pragma warning restore CS0162 // Unreachable code detected

			ArrangeAllocation(new Rectangle(Point.Zero, mAllocation.Size));
			AllocateChildren(mAllocation);

			if (virtualView.Frame != mAllocation)
			{
				// IsSizeAllocating = true;
				//
				// Arrange(mAllocation);
			}

			base.OnSizeAllocated(allocation);
		}
		finally
		{
			CurrentAllocation = null;
			IsReallocating = false;
			IsSizeAllocating = false;
		}
	}

	IEnumerable<(Widget w, IView view)> GetItemViews()
	{
		if (Adaptor is not { })
			yield break;

		foreach (var cr in _items.ToArray())
		{
			var w = cr;

			if (w is ViewHolder vw && Adaptor.GetTemplatedView(vw.Child) is { } v)
			{
				yield return (w, v);
			}
		}
	}

	void AllocateChildren(Rect allocation)
	{
		if (Adaptor is not { })
			return;

		foreach (var cr in GetItemViews())
		{
			var (w, v) = cr;
			var r = v.Frame;

			if (r.IsEmpty)
				continue;

			var cAlloc = new Gdk.Rectangle((int)(allocation.X + r.X), (int)(allocation.Y + r.Y), (int)r.Width, (int)r.Height);

			if (w is ViewHolder holder)
			{
				cAlloc = new Rect(new Point(allocation.X + holder.Bounds.X, allocation.Y + holder.Bounds.Y), holder.Bounds.Size).ToNative();
			}

			// if (cAlloc != w.Allocation) // it's always needed to implicit arrange children:
			w.SizeAllocate(cAlloc);
		}
	}

	void ArrangeAllocation(Rectangle allocation)
	{
		if (VirtualView is not { } virtualView || LayoutManager is not { })
			return;

		LayoutManager.SizeAllocated(allocation.Size);
		LayoutManager.LayoutItems(new Rect(Point.Zero, allocation.Size), false);

		foreach (var cr in GetItemViews())
		{
			var (w, v) = cr;
		}
	}

	public bool IsMeasuring { get; set; }

	Size Measure(double widthConstraint, double heightConstraint)
	{
		IsMeasuring = true;

		try
		{
			if (VirtualView is not { } virtualView || LayoutManager is not { })
				return new Size(widthConstraint, heightConstraint);

			var size = new Size(widthConstraint, heightConstraint);
			// LayoutManager.Reset();
			LayoutManager.SizeAllocated(size);
			LayoutManager.LayoutItems(new Rect(Point.Zero, size), false);

			var measured = LayoutManager.GetScrollCanvasSize();
			var blockSize = LayoutManager.GetScrollBlockSize();

			var width = LayoutManager.GetScrollColumnSize();

			if (double.IsPositiveInfinity(measured.Width))
				measured.Width = width;

			if (double.IsPositiveInfinity(measured.Height))
				measured.Height = blockSize;

			return measured;
		}
		finally
		{
			IsMeasuring = false;
		}
	}

	public Size Clamp(Size mesured, Size constraint)
	{
		var w = mesured.Width;
		var h = mesured.Height;

		if (double.IsPositiveInfinity(mesured.Width))
			w = constraint.Width;

		if (double.IsPositiveInfinity(mesured.Height))
			h = constraint.Height;

		return new(w, h);
	}

	bool RestrictToMeasuredArrange = false;

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

		var arrangeSize = RestrictToMeasuredArrange ? Measure(rect.Width, rect.Height) : rect.Size;
		var alloc = new Rectangle(rect.Location, arrangeSize);
		SizeAllocate(alloc.ToNative());
		QueueAllocate();
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
#if !USE_ADJUSTSIZEREQUEST

	// protected override SizeRequestMode OnGetRequestMode()
	// {
	// 	// dirty fix: unwrapped labels report fixed sizes, forcing parents to fixed mode
	// 	//            -> report always width_for_height, since we don't support angles
	// 	return Gtk.SizeRequestMode.WidthForHeight;
	// }

	double? RequestedHeight { get; set; }

	double? RequestedWidth { get; set; }

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
	protected Size MeasureMinimum()
	{
		if (MeasuredMinimum != null)
			return MeasuredMinimum.Value;

		if (VirtualView is not { } virtualView || LayoutManager is not { } || Adaptor is not { })
			return Size.Zero;

		IsMeasuring = true;
		var itemSize = CollectionViewController.GetItemSize(double.PositiveInfinity, double.PositiveInfinity);
		var firstAlloc = new Size(double.PositiveInfinity, itemSize.Height);
		LayoutManager.SizeAllocated(firstAlloc);
		var blockSize = LayoutManager.GetScrollBlockSize();

		var width = LayoutManager.GetScrollColumnSize();
		firstAlloc.Width = width;
		LayoutManager.SizeAllocated(firstAlloc);
		LayoutManager.LayoutItems(new Rect(Point.Zero, firstAlloc), false);

		blockSize = LayoutManager.GetScrollBlockSize();

		width = LayoutManager.GetScrollColumnSize();

		// var desiredSize = GetChildrensView().Select(c => c.view)
		//  .Aggregate(new Size(),
		// (s, c) => new Size(Math.Max(s.Width, c.DesiredSize.Width), s.Height + c.DesiredSize.Height));

		IsMeasuring = false;
		MeasuredMinimum = new Size(width, blockSize);

		return MeasuredMinimum.Value;
	}

	// protected override Gtk.SizeRequestMode OnGetRequestMode()
	// {
	// 	// dirty fix: unwrapped labels report fixed sizes, forcing parents to fixed mode
	// 	//            -> report always width_for_height, since we don't support angles
	// 	return Gtk.SizeRequestMode.WidthForHeight;
	// }

	protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
	{
		base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

		if (IsSizeAllocating)
		{
			return;
		}

		if (VirtualView is not { } virtualView)
			return;

		var measuredMinimum = MeasureMinimum();

		double constraint = minimumSize;

		if (orientation == Orientation.Horizontal)
		{
			if (RequestMode is SizeRequestMode.WidthForHeight or SizeRequestMode.ConstantSize)
			{
				if (MeasuredSizeV is { Width : > 0 } size && (constraint == 0))
					constraint = size.Width;

				constraint = constraint == 0 ? double.PositiveInfinity : constraint;
			}
			else
			{
				;
			}

			MeasuredSizeH = constraint != 0 ? Measure(constraint, double.PositiveInfinity) : measuredMinimum;

			constraint = MeasuredSizeH.Value.Width;

			minimumSize = (int)measuredMinimum.Width;
			naturalSize = (int)constraint;
		}

		if (orientation == Orientation.Vertical)
		{
			var widthContraint = double.PositiveInfinity;

			if (RequestMode is SizeRequestMode.HeightForWidth or SizeRequestMode.ConstantSize)
			{
				MeasuredSizeH ??= measuredMinimum;

				if (MeasuredSizeH is { } size && constraint == 0)
				{
					if (size.Height > 0)
						constraint = size.Height;

					if (size.Width > 0)
						widthContraint = size.Width;
				}

				constraint = constraint == 0 ? double.PositiveInfinity : constraint;
			}
			else
			{
				;
			}

			MeasuredSizeV = constraint != 0 ? Measure(widthContraint, constraint) : measuredMinimum;

			constraint = MeasuredSizeV.Value.Height;

			minimumSize = (int)measuredMinimum.Height;
			naturalSize = (int)constraint;
		}
	}

#endif
}