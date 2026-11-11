#nullable enable

using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{

	public partial class Element
	{

		[MissingMapper("accessibility on GTK is defined with Atk; the MAUI accessibility tree flags have no direct equivalent")]
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, IElement element)
		{ }

		[MissingMapper("accessibility on GTK is defined with Atk; the MAUI accessibility tree flags have no direct equivalent")]
		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, IElement element)
		{ }

	}

}