using Gtk;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	/// <summary>
	/// The GTK shell hosts the section's navigation view; the ShellContent
	/// handler only needs a platform element so the shell can map it.
	/// </summary>
	public partial class ShellContentHandler : ElementHandler<ShellContent, Widget>
	{
		public static PropertyMapper<ShellContent, ShellContentHandler> Mapper =
			new PropertyMapper<ShellContent, ShellContentHandler>(ElementMapper);

		public static CommandMapper<ShellContent, ShellContentHandler> CommandMapper =
			new CommandMapper<ShellContent, ShellContentHandler>(ElementCommandMapper);

		public ShellContentHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override Widget CreatePlatformElement() => new Gtk.Box(Gtk.Orientation.Vertical, 0);
	}
}
