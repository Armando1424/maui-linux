using Gtk;
using PlatformView = Microsoft.Maui.Platform.MauiMenuItem;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSeparatorHandler
	{
		protected override PlatformView CreatePlatformElement()
		{
			return new PlatformView { IsSeparator = true };
		}
	}
}
