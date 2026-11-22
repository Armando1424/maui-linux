namespace Microsoft.Maui.Platform;

/// GTK4 eliminó GtkButton::image: el contenido del botón es un hijo (Child).
public class MauiImageButton : Gtk.Button
{
	ImageView? _imageView;

	public ImageView ImageView
	{
		get
		{
			if (_imageView is null)
			{
				_imageView = new ImageView();
				Child = _imageView;
			}

			return _imageView;
		}
		set
		{
			_imageView = value;
			Child = value;
		}
	}
}
