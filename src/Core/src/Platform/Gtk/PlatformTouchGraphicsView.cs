using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

/// <summary>
/// GTK4 eliminó los virtuales de eventos (OnButtonPressEvent, OnMotionNotifyEvent…):
/// ahora se usan controladores. La lógica de interacción es la misma; solo cambia
/// de dónde vienen las coordenadas.
/// </summary>
public class PlatformTouchGraphicsView : Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView
{
	IGraphicsView? _graphicsView;
	bool _isTouching;
	bool _isInBounds;

	public PlatformTouchGraphicsView()
	{
		var click = new Gtk.GestureClick();
		click.Pressed += (_, e) =>
		{
			_isTouching = true;
			_graphicsView?.StartInteraction(ToPoints(e.X, e.Y));
		};
		click.Released += (_, e) =>
		{
			_isTouching = false;
			_graphicsView?.EndInteraction(ToPoints(e.X, e.Y), _isInBounds);
		};
		AddController(click);

		var motion = new Gtk.EventControllerMotion();
		motion.Enter += (_, e) =>
		{
			_isInBounds = true;
			_graphicsView?.StartHoverInteraction(ToPoints(e.X, e.Y));
		};
		motion.Motion += (_, e) =>
		{
			_graphicsView?.MoveHoverInteraction(ToPoints(e.X, e.Y));
		};
		motion.Leave += (_, _) =>
		{
			_isInBounds = false;
			_graphicsView?.EndHoverInteraction();

			if (_isTouching)
			{
				_isTouching = false;
				_graphicsView?.EndInteraction(System.Array.Empty<PointF>(), _isInBounds);
			}
		};
		AddController(motion);
	}

	static PointF[] ToPoints(double x, double y) => new[] { new PointF((float)x, (float)y) };

	public void Connect(IGraphicsView graphicsView)
	{
		_graphicsView = graphicsView;
	}

	public void Disconnect()
	{
		_graphicsView = null;
	}
}
