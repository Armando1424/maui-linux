using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Pango;
using Xunit;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class LabelHandlerTests
{
    private static LabelHandler CreateHandler(MauiLabel label)
    {
        var handler = new LabelHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(label);
        return handler;
    }

    [GtkFact]
    public void MapsText()
    {
        var label = new MauiLabel { Text = "Hello" };

        var handler = CreateHandler(label);

        Assert.Equal("Hello", handler.PlatformView!.Text);
    }

    [GtkFact]
    public void MapsHorizontalTextAlignment()
    {
        var label = new MauiLabel { Text = "Hello", HorizontalTextAlignment = TextAlignment.Center };

        var handler = CreateHandler(label);

        Assert.Equal(global::Gtk.Justification.Center, handler.PlatformView!.Justify);
    }

    [GtkFact]
    public void MapsLineBreakMode()
    {
        var label = new MauiLabel { Text = "Hello", LineBreakMode = LineBreakMode.CharacterWrap };

        var handler = CreateHandler(label);

        Assert.True(handler.PlatformView!.LineWrap);
        Assert.Equal(EllipsizeMode.None, handler.PlatformView.Ellipsize);
    }

    [GtkFact]
    public void UpdatingText_UpdatesPlatformView()
    {
        var label = new MauiLabel { Text = "Before" };
        var handler = CreateHandler(label);

        label.Text = "After";
        handler.UpdateValue(nameof(MauiLabel.Text));

        Assert.Equal("After", handler.PlatformView!.Text);
    }

    [GtkFact]
    public void MapsTextDecorations()
    {
        var label = new MauiLabel { Text = "Hello", TextDecorations = TextDecorations.Underline };

        var handler = CreateHandler(label);

        Assert.NotNull(handler.PlatformView!.Attributes);
    }
}
