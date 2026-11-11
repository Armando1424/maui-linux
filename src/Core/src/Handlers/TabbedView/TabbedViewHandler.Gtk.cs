using PlatformView = Microsoft.Maui.Platform.TabbedView;

namespace Microsoft.Maui.Handlers;

public partial class TabbedViewHandler
{
	protected override PlatformView CreatePlatformView()
	{
		return new();
	}
}