using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiActivityIndicator = Microsoft.Maui.Controls.ActivityIndicator;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class ActivityIndicatorHandlerTests
{
    private static ActivityIndicatorHandler CreateHandler(MauiActivityIndicator indicator)
    {
        var handler = new ActivityIndicatorHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(indicator);
        return handler;
    }

    [GtkFact]
    public void MapsIsRunning()
    {
        var indicator = new MauiActivityIndicator { IsRunning = true };

        var handler = CreateHandler(indicator);

        Assert.True(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void MapsIsRunningFalse()
    {
        var indicator = new MauiActivityIndicator { IsRunning = false };

        var handler = CreateHandler(indicator);

        Assert.False(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void UpdatingIsRunning_UpdatesPlatformView()
    {
        var indicator = new MauiActivityIndicator();
        var handler = CreateHandler(indicator);

        indicator.IsRunning = true;
        handler.UpdateValue(nameof(MauiActivityIndicator.IsRunning));

        Assert.True(handler.PlatformView!.Active);
    }
}
