using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Handlers
{
	// https://docs.gtk.org/gtk3/class.Button.html

	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreatePlatformView()
		{
			return Button.NewWithLabel(string.Empty);
		}

		protected override void ConnectHandler(Button nativeView)
		{
			nativeView.Clicked += OnButtonClicked;
			nativeView.ButtonPressEvent += OnButtonPressEvent;
			nativeView.ButtonReleaseEvent += OnButtonReleaseEvent;
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Clicked -= OnButtonClicked;
			nativeView.ButtonPressEvent -= OnButtonPressEvent;
			nativeView.ButtonReleaseEvent -= OnButtonReleaseEvent;
		}

		public static void MapText(IButtonHandler handler, ITextButton button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button.TextColor);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			if (handler.PlatformView.Child is Label nativeView)
			{
				nativeView.Attributes = nativeView.Attributes.AttrListFor(button.CharacterSpacing);
			}
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapImageSourceAsync(IButtonHandler handler, IImage image)
		{
			if (handler.PlatformView is { })
				await handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView.WithPadding(button.Padding);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke) =>
			handler.PlatformView?.UpdateBorderColor(buttonStroke.StrokeColor);

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke) =>
			handler.PlatformView?.UpdateBorderWidth(buttonStroke.StrokeThickness);

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			if (handler.PlatformView is { } platformView)
				platformView.UpdateCornerRadius(new CornerRadius(buttonStroke.CornerRadius));
		}

		void OnButtonPressEvent(object? o, ButtonPressEventArgs args)
		{
			InvokeEvent(() => VirtualView?.Pressed());
		}

		void OnButtonReleaseEvent(object? o, ButtonReleaseEventArgs args)
		{
			InvokeEvent(() => VirtualView?.Released());
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			InvokeEvent(() => VirtualView?.Clicked());
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(Gdk.Pixbuf? platformImage)
			{
				if (Handler?.PlatformView is not Button button)
					return;

				button.Image = new Image(platformImage);
			}
		}
	}
}