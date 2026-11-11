using System.Linq;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

/// <summary>
/// Covers the mappings that were added for the handlers that used to be
/// declared as <c>[MissingMapper]</c>.
/// </summary>
[Collection(GtkCollection.Name)]
public class MappingTests
{
	[GtkFact]
	public void ImageAspectIsApplied()
	{
		var image = new Image { Aspect = Aspect.AspectFill };
		var handler = new ImageHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(image);

		Assert.Equal(Aspect.AspectFill, handler.PlatformView!.Aspect);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void EditorPlaceholderFollowsTheBuffer()
	{
		var editor = new Editor { Placeholder = "Escribe aquí" };
		var handler = new EditorHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(editor);

		var placeholder = handler.PlatformView!.Children.OfType<global::Gtk.Label>().Single();
		Assert.Equal("Escribe aquí", placeholder.Text);
		Assert.True(placeholder.Visible);

		editor.Text = "contenido";
		Assert.False(placeholder.Visible);

		editor.Text = string.Empty;
		Assert.True(placeholder.Visible);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void EditorMaxLengthStopsManualInsertion()
	{
		var editor = new Editor { MaxLength = 5 };
		var handler = new EditorHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(editor);

		editor.Text = "abc";

		var buffer = handler.PlatformView!.Buffer;
		buffer.Insert(buffer.EndIter, "defgh");

		Assert.Equal(5, buffer.CharCount);
		Assert.Equal("abcde", buffer.Text);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void EditorHorizontalAlignmentIsMapped()
	{
		var editor = new Editor { HorizontalTextAlignment = TextAlignment.Center };
		var handler = new EditorHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(editor);

		Assert.Equal(global::Gtk.Justification.Center, handler.PlatformView!.Justification);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void EntryClearButtonVisibilityIsMapped()
	{
		var entry = new Entry { ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
		var handler = new EntryHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(entry);

		Assert.Equal("edit-clear-symbolic", handler.PlatformView!.SecondaryIconName);

		entry.ClearButtonVisibility = ClearButtonVisibility.Never;
		Assert.Null(handler.PlatformView.SecondaryIconName);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void PickerTitleIsMappedToTooltip()
	{
		var picker = new Picker { Title = "Elige uno", ItemsSource = new[] { "a", "b" } };
		var handler = new PickerHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(picker);

		Assert.Equal("Elige uno", handler.PlatformView!.TooltipText);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void RefreshViewShowsContentAndSpinner()
	{
		var content = new Label { Text = "contenido" };
		var refresh = new RefreshView { Content = content, IsRefreshing = true };
		var handler = new RefreshViewHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(refresh);

		Assert.True(handler.PlatformView!.IsRefreshing);
		Assert.True(handler.PlatformView.Spinner.Visible);
		Assert.NotNull(handler.PlatformView.Content);

		refresh.IsRefreshing = false;
		Assert.False(handler.PlatformView.Spinner.Visible);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void IndicatorViewMapsCountAndPosition()
	{
		var indicator = new IndicatorView { Count = 4, Position = 2, IndicatorSize = 8 };
		var handler = new IndicatorViewHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(indicator);

		Assert.Equal(4, handler.PlatformView!.Count);
		Assert.Equal(2, handler.PlatformView.Position);
		Assert.Equal(8, handler.PlatformView.IndicatorSize);

		indicator.Position = 3;
		Assert.Equal(3, handler.PlatformView.Position);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void TimePickerUsesTheFormat()
	{
		var picker = new TimePicker { Time = new System.TimeSpan(14, 30, 0), Format = "HH:mm" };
		var handler = new TimePickerHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(picker);

		Assert.Equal("14:30", handler.PlatformView!.Text);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void LayoutZIndexReordersTheChildren()
	{
		var first = new Label { Text = "first" };
		var second = new Label { Text = "second" };
		var layout = new VerticalStackLayout { Children = { first, second } };

		var handler = new LayoutHandler();
		handler.SetMauiContext(HandlerTestContext.Context);
		handler.SetVirtualView(layout);

		var nativeFirst = (global::Gtk.Widget)first.Handler!.PlatformView!;
		var nativeSecond = (global::Gtk.Widget)second.Handler!.PlatformView!;

		Assert.Same(nativeFirst, handler.PlatformView!.Children[0]);

		first.ZIndex = 1;
		handler.UpdateZIndex(first);

		Assert.Same(nativeSecond, handler.PlatformView.Children[0]);
		Assert.Same(nativeFirst, handler.PlatformView.Children[1]);

		((IElementHandler)handler).DisconnectHandler();
	}

	[GtkFact]
	public void PageTitleIsAppliedToTheWindow()
	{
		var window = new global::Gtk.Window("page-title");
		var page = new ContentPage { Title = "Mi página", Content = new Label { Text = "x" } };

		var handler = new PageHandler();
		handler.SetMauiContext(HandlerTestContext.CreateWithWindow(window));
		handler.SetVirtualView(page);

		Assert.Equal("Mi página", window.Title);

		((IElementHandler)handler).DisconnectHandler();
	}
}
