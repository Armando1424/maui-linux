using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Microsoft.Maui.Graphics;
using Xunit;
using MauiSwitch = Microsoft.Maui.Controls.Switch;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class SwitchHandlerTests
{
    private static SwitchHandler CreateHandler(MauiSwitch virtualSwitch)
    {
        var handler = new SwitchHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(virtualSwitch);
        return handler;
    }

    [GtkFact]
    public void MapsIsOn()
    {
        var virtualSwitch = new MauiSwitch { IsToggled = true };

        var handler = CreateHandler(virtualSwitch);

        Assert.True(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void DefaultsToOff()
    {
        var virtualSwitch = new MauiSwitch();

        var handler = CreateHandler(virtualSwitch);

        Assert.False(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void UpdatingIsOn_UpdatesPlatformView()
    {
        var virtualSwitch = new MauiSwitch();
        var handler = CreateHandler(virtualSwitch);

        virtualSwitch.IsToggled = true;
        handler.UpdateValue(nameof(MauiSwitch.IsToggled));

        Assert.True(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void MapsThumbColor()
    {
        var virtualSwitch = new MauiSwitch { ThumbColor = Colors.Red };

        var exception = Record.Exception(() => CreateHandler(virtualSwitch));

        Assert.Null(exception);
    }
}
