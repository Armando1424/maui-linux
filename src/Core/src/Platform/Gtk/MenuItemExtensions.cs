using System.Collections.Generic;

namespace Microsoft.Maui.Platform;

public static class MenuItemExtensions
{
	[MissingMapper]
	public static void UpdateKeyboardAccelerators(this MauiMenuItem platformView, IReadOnlyList<IKeyboardAccelerator>? viewKeyboardAccelerators)
	{
	}

	[MissingMapper]
	public static void UpdateImageSource(this MauiMenuItem platformView, IImageSource? viewSource)
	{
	}

	public static void UpdateText(this MauiMenuItem platformView, string text)
	{
		platformView.UpdateText(text);
	}
}