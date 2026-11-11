using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiCheckBox = Microsoft.Maui.Controls.CheckBox;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class CheckBoxHandlerTests
{
    private static CheckBoxHandler CreateHandler(MauiCheckBox checkBox)
    {
        var handler = new CheckBoxHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(checkBox);
        return handler;
    }

    [GtkFact]
    public void MapsIsChecked()
    {
        var checkBox = new MauiCheckBox { IsChecked = true };

        var handler = CreateHandler(checkBox);

        Assert.True(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void DefaultsToUnchecked()
    {
        var checkBox = new MauiCheckBox();

        var handler = CreateHandler(checkBox);

        Assert.False(handler.PlatformView!.Active);
    }

    [GtkFact]
    public void NativeToggle_UpdatesVirtualView()
    {
        var checkBox = new MauiCheckBox();
        var handler = CreateHandler(checkBox);

        handler.PlatformView!.Active = true;
        HandlerTestContext.Pump();

        Assert.True(checkBox.IsChecked);
    }

    [GtkFact]
    public void UpdatingIsChecked_UpdatesPlatformView()
    {
        var checkBox = new MauiCheckBox();
        var handler = CreateHandler(checkBox);

        checkBox.IsChecked = true;
        handler.UpdateValue(nameof(MauiCheckBox.IsChecked));

        Assert.True(handler.PlatformView!.Active);
    }
}
