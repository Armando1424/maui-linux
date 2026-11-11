using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class LayoutHandlerTests
{
    private static LayoutHandler CreateHandler(VerticalStackLayout layout)
    {
        var handler = new LayoutHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(layout);
        return handler;
    }

    [GtkFact]
    public void AddsChildToPlatformView()
    {
        var layout = new VerticalStackLayout();
        layout.Add(new MauiLabel { Text = "child" });

        var handler = CreateHandler(layout);

        Assert.Single(handler.PlatformView!.Children);
    }

    [GtkFact]
    public void AddsMultipleChildren()
    {
        var layout = new VerticalStackLayout();
        layout.Add(new MauiLabel { Text = "one" });
        layout.Add(new MauiLabel { Text = "two" });
        layout.Add(new MauiLabel { Text = "three" });

        var handler = CreateHandler(layout);

        Assert.Equal(3, handler.PlatformView!.Children.Length);
    }

    [GtkFact]
    public void RemoveRemovesChild()
    {
        var child = new MauiLabel { Text = "child" };
        var layout = new VerticalStackLayout();
        layout.Add(child);
        var handler = CreateHandler(layout);

        handler.Remove(child);

        Assert.Empty(handler.PlatformView!.Children);
    }

    [GtkFact]
    public void ClearRemovesAllChildren()
    {
        var layout = new VerticalStackLayout();
        layout.Add(new MauiLabel { Text = "one" });
        layout.Add(new MauiLabel { Text = "two" });
        var handler = CreateHandler(layout);

        handler.Clear();

        Assert.Empty(handler.PlatformView!.Children);
    }

    [GtkFact]
    public void InsertAddsChildAtPosition()
    {
        var layout = new VerticalStackLayout();
        layout.Add(new MauiLabel { Text = "first" });
        var handler = CreateHandler(layout);

        handler.Insert(1, new MauiLabel { Text = "second" });

        Assert.Equal(2, handler.PlatformView!.Children.Length);
    }
}
