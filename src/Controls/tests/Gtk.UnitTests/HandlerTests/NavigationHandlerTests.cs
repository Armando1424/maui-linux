using System;
using System.Linq;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

/// <summary>
/// Platform level navigation tests. They drive <see cref="NavigationView"/>
/// directly (instead of <see cref="NavigationPage.PushAsync"/>, which needs the
/// full application lifecycle) so the GTK stack management is deterministic.
/// </summary>
[Collection(GtkCollection.Name)]
public class NavigationHandlerTests
{
	private static (NavigationPage Page, NavigationView Native, ContentPage Root) CreateNavigation()
	{
		var context = HandlerTestContext.CreateWithWindow(new global::Gtk.Window("navigation"));
		var root = new ContentPage
		{
			Title = "Root",
			Content = new Label { Text = "root" },
		};

		var page = new NavigationPage(root);

		// NavigationView only needs the handler to reach the MauiContext.
		var pageHandler = new TestElementHandler();
		pageHandler.SetMauiContext(context);
		((IElement)page).Handler = pageHandler;

		var native = new NavigationView();
		native.Connect(page);

		return (page, native, root);
	}

	private static ContentPage CreatePage(string title)
		=> new()
		{
			Title = title,
			Content = new Label { Text = title },
		};

	private static void Navigate(NavigationView native, params IView[] stack)
		=> native.RequestNavigation(new NavigationRequest(stack, false));

	private static global::Gtk.Widget WidgetOf(IView view)
	{
		Assert.NotNull(view.Handler);
		return Assert.IsAssignableFrom<global::Gtk.Widget>(view.Handler!.PlatformView);
	}

	[GtkFact]
	public void RootPageBecomesTheOnlyChild()
	{
		var (_, native, root) = CreateNavigation();

		Navigate(native, root);

		Assert.Single(native.Children);
		Assert.Same(WidgetOf(root), native.Children.Single());
	}

	[GtkFact]
	public void PushShowsTheTopPage()
	{
		var (_, native, root) = CreateNavigation();
		var second = CreatePage("Second");

		Navigate(native, root);
		Navigate(native, root, second);

		Assert.Single(native.Children);
		Assert.Same(WidgetOf(second), native.Children.Single());
		Assert.NotNull(second.Handler);
	}

	[GtkFact]
	public void PushReplacesTheVisibleWidget()
	{
		var (_, native, root) = CreateNavigation();
		var second = CreatePage("Second");
		var third = CreatePage("Third");

		Navigate(native, root);
		var rootWidget = native.Children.Single();

		Navigate(native, root, second);
		var secondWidget = native.Children.Single();

		Navigate(native, root, second, third);
		var thirdWidget = native.Children.Single();

		Assert.NotSame(rootWidget, secondWidget);
		Assert.NotSame(secondWidget, thirdWidget);
	}

	[GtkFact]
	public void PopRestoresThePreviousWidget()
	{
		var (_, native, root) = CreateNavigation();
		var second = CreatePage("Second");

		Navigate(native, root);
		var rootWidget = native.Children.Single();

		Navigate(native, root, second);
		Navigate(native, root);

		Assert.Single(native.Children);
		Assert.Same(rootWidget, native.Children.Single());
	}

	[GtkFact]
	public void PoppedPageIsDisconnected()
	{
		var (_, native, root) = CreateNavigation();
		var second = CreatePage("Second");

		Navigate(native, root);
		Navigate(native, root, second);

		Assert.NotNull(second.Handler);

		Navigate(native, root);

		Assert.Null(second.Handler);
	}

	[GtkFact]
	public void RepeatedPushPopKeepsASingleChild()
	{
		var (_, native, root) = CreateNavigation();
		Navigate(native, root);

		for (var i = 0; i < 25; i++)
		{
			var page = CreatePage($"Page {i}");

			Navigate(native, root, page);
			Assert.Single(native.Children);

			Navigate(native, root);
			Assert.Single(native.Children);
			Assert.Null(page.Handler);
		}
	}
}
