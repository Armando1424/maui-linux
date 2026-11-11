using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public class ShellHandlerTests
{
	[GtkFact]
	public void ShellShowsTheCurrentSection()
	{
		var shell = new Shell();
		var item = new ShellItem { Title = "Inicio" };
		var section = new ShellSection { Title = "Principal" };
		var content = new ShellContent
		{
			Content = new ContentPage { Content = new Label { Text = "hola" } },
		};

		section.Items.Add(content);
		item.Items.Add(section);
		shell.Items.Add(item);

		var window = new global::Gtk.Window("shell");
		var handler = new ShellHandler();
		handler.SetMauiContext(HandlerTestContext.CreateWithWindow(window));
		handler.SetVirtualView(shell);

		var view = handler.PlatformView!;

		Assert.NotNull(view);
		Assert.True(view.Children.Length > 0);

		// flyout + content area
		Assert.Equal(2, view.Children.Length);

		var contentArea = Assert.IsType<global::Gtk.Box>(view.Children[1]);
		Assert.True(contentArea.Children.Length >= 2);
		Assert.NotNull(shell.CurrentItem);

		((IElementHandler)handler).DisconnectHandler();
	}
}
