using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreatePlatformView()
		{
			return new MauiTimePicker();
		}

		public static void MapFormat(ITimePickerHandler handler, ITimePicker view)
		{
			handler.PlatformView?.UpdateFormat(view);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker view)
		{
			handler.PlatformView?.UpdateTime(view);
		}

		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker view)
		{
			if (handler.PlatformView is { } platformView)
				platformView.Attributes = platformView.Attributes.AttrListFor(view.CharacterSpacing);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(view, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker.TextColor);
		}
	}
}