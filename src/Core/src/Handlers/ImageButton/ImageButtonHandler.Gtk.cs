using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreatePlatformView() => new();

		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke) =>
			handler.PlatformView?.UpdateBorderColor(buttonStroke.StrokeColor);

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke) =>
			handler.PlatformView?.UpdateBorderWidth(buttonStroke.StrokeThickness);

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			if (handler.PlatformView is { } platformView)
				platformView.UpdateCornerRadius(new CornerRadius(buttonStroke.CornerRadius));
		}

		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton) =>
			handler.PlatformView.WithPadding(imageButton.Padding);

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(Gdk.Pixbuf? platformImage)
			{
				if (Handler?.PlatformView is not MauiImageButton button)
					return;

				var imageView = button.ImageView ?? new();
				imageView.Image = platformImage;
				button.ImageView = imageView;
			}
		}
	}
}