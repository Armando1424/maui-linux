using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiEntry = Microsoft.Maui.Controls.Entry;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class EntryHandlerTests
{
    private static EntryHandler CreateHandler(MauiEntry entry)
    {
        var handler = new EntryHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(entry);
        return handler;
    }

    [GtkFact]
    public void MapsText()
    {
        var entry = new MauiEntry { Text = "hello" };

        var handler = CreateHandler(entry);

        Assert.Equal("hello", handler.PlatformView!.Text);
    }

    [GtkFact]
    public void MapsPlaceholder()
    {
        var entry = new MauiEntry { Placeholder = "Type your name" };

        var handler = CreateHandler(entry);

        Assert.Equal("Type your name", handler.PlatformView!.PlaceholderText);
    }

    [GtkFact]
    public void MapsMaxLength()
    {
        var entry = new MauiEntry { MaxLength = 12 };

        var handler = CreateHandler(entry);

        Assert.Equal(12, handler.PlatformView!.MaxLength);
    }

    [GtkFact]
    public void MapsIsPassword()
    {
        var entry = new MauiEntry { IsPassword = true };

        var handler = CreateHandler(entry);

        Assert.False(handler.PlatformView!.Visibility);
    }

    [GtkFact]
    public void MapsIsReadOnly()
    {
        var entry = new MauiEntry { IsReadOnly = true };

        var handler = CreateHandler(entry);

        Assert.False(handler.PlatformView!.IsEditable);
    }

    [GtkFact]
    public void EmptyText_RoundTrips()
    {
        var entry = new MauiEntry { Text = "something" };
        var handler = CreateHandler(entry);

        entry.Text = string.Empty;
        handler.UpdateValue(nameof(MauiEntry.Text));

        Assert.Equal(string.Empty, handler.PlatformView!.Text);
    }

    [GtkFact]
    public void NativeTextChange_UpdatesVirtualView()
    {
        var entry = new MauiEntry();
        var handler = CreateHandler(entry);

        handler.PlatformView!.Text = "typed";
        HandlerTestContext.Pump();

        Assert.Equal("typed", entry.Text);
    }

    [GtkFact]
    public void UpdatingText_UpdatesPlatformView()
    {
        var entry = new MauiEntry { Text = "Before" };
        var handler = CreateHandler(entry);

        entry.Text = "After";
        handler.UpdateValue(nameof(MauiEntry.Text));

        Assert.Equal("After", handler.PlatformView!.Text);
    }
}
