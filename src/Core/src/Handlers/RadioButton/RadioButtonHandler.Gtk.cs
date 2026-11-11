using Gtk;
using System;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, RadioButton>
	{
		static RadioButton baseRadioButton = new("base");

		protected override RadioButton CreatePlatformView()
		{
			return new RadioButton(baseRadioButton, "foo");
		}

		protected override void ConnectHandler(RadioButton platformView)
		{
			platformView.Toggled += OnClicked;
		}

		protected override void DisconnectHandler(RadioButton platformView)
		{
			platformView.Toggled -= OnClicked;
		}

		public static void MapBackground(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (handler.PlatformView is RadioButton rb)
				rb.UpdateBackground(radioButton);
		}

		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (handler.PlatformView is RadioButton rb)
				rb.UpdateIsChecked(radioButton);
		}

		public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (handler.PlatformView is RadioButton rb)
				rb.UpdateContent(radioButton);
		}

		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.PlatformView?.UpdateTextColor(textStyle.TextColor);
		}

		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			if (handler.PlatformView is RadioButton radioButton && radioButton.Child is Label label)
				label.Attributes = label.Attributes.AttrListFor(textStyle.CharacterSpacing);
		}

		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(textStyle, fontManager);
		}

		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (handler.PlatformView is { } platformView)
				platformView.SetStyleColor(radioButton.StrokeColor, "radio", "border-color");
		}

		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (radioButton.StrokeThickness < 0)
				return;

			handler.PlatformView?.SetStyleValueNode(
				$"{radioButton.StrokeThickness:0.##}px", "radiobutton", "border-width", "radio");
		}

		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (handler.PlatformView is { } platformView)
				platformView.SetStyleValueNode(
					$"{radioButton.CornerRadius:0.##}px", "radiobutton", "border-radius", "radio");
		}

		void OnClicked(object? sender, EventArgs e)
		{
			if (sender is RadioButton platformView && VirtualView != null)
				VirtualView.IsChecked = platformView.Active;
		}
	}
}