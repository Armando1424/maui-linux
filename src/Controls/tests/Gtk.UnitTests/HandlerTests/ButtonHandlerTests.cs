using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiButton = Microsoft.Maui.Controls.Button;
using GtkButton = Gtk.Button;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class ButtonHandlerTests
{
    private static ButtonHandler CreateHandler(MauiButton button)
    {
        var handler = new ButtonHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(button);
        return handler;
    }

    [GtkFact]
    public void MapsText()
    {
        var button = new MauiButton { Text = "Save" };

        var handler = CreateHandler(button);

        Assert.Equal("Save", handler.PlatformView!.Label);
    }

    [GtkFact]
    public void MapsIsEnabled()
    {
        var button = new MauiButton { Text = "Save", IsEnabled = false };

        var handler = CreateHandler(button);

        Assert.False(handler.PlatformView!.Sensitive);
    }

    [GtkFact]
    public void Click_RaisesClickedOnVirtualView()
    {
        var button = new MauiButton { Text = "Click" };
        var handler = CreateHandler(button);
        var fired = false;
        button.Clicked += (_, _) => fired = true;

        handler.PlatformView!.Click();
        HandlerTestContext.Pump();

        Assert.True(fired);
    }

    [GtkFact]
    public void Click_ExecutesCommand()
    {
        var executions = 0;
        var button = new MauiButton
        {
            Text = "Click",
            Command = new Command(() => executions++),
        };
        var handler = CreateHandler(button);

        handler.PlatformView!.Click();
        HandlerTestContext.Pump();

        Assert.Equal(1, executions);
    }

    [GtkFact]
    public void Click_PassesCommandParameter()
    {
        object? received = null;
        var button = new MauiButton
        {
            Text = "Click",
            CommandParameter = "payload",
            Command = new Command(parameter => received = parameter),
        };
        var handler = CreateHandler(button);

        handler.PlatformView!.Click();
        HandlerTestContext.Pump();

        Assert.Equal("payload", received);
    }

    [GtkFact]
    public void Click_WhenCanExecuteIsFalse_DoesNotExecute()
    {
        var executions = 0;
        var button = new MauiButton
        {
            Text = "Click",
            Command = new Command(() => executions++, () => false),
        };
        var handler = CreateHandler(button);

        handler.PlatformView!.Click();
        HandlerTestContext.Pump();

        Assert.Equal(0, executions);
    }

    [GtkFact]
    public void UpdatingText_UpdatesPlatformView()
    {
        var button = new MauiButton { Text = "Before" };
        var handler = CreateHandler(button);

        button.Text = "After";
        handler.UpdateValue(nameof(MauiButton.Text));

        Assert.Equal("After", handler.PlatformView!.Label);
    }
}
