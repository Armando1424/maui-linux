using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiSlider = Microsoft.Maui.Controls.Slider;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class SliderHandlerTests
{
    private static SliderHandler CreateHandler(MauiSlider slider)
    {
        var handler = new SliderHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(slider);
        return handler;
    }

    [GtkFact]
    public void MapsMinimumAndMaximum()
    {
        var slider = new MauiSlider { Minimum = -10, Maximum = 10 };

        var handler = CreateHandler(slider);

        Assert.Equal(-10, handler.PlatformView!.Adjustment.Lower);
        Assert.Equal(10, handler.PlatformView.Adjustment.Upper);
    }

    [GtkFact]
    public void MapsValue()
    {
        var slider = new MauiSlider { Minimum = 0, Maximum = 10, Value = 4 };

        var handler = CreateHandler(slider);

        Assert.Equal(4, handler.PlatformView!.Value, precision: 3);
    }

    [GtkFact]
    public void NativeValueChange_UpdatesVirtualView()
    {
        var slider = new MauiSlider { Minimum = 0, Maximum = 10 };
        var handler = CreateHandler(slider);

        handler.PlatformView!.Value = 7;
        HandlerTestContext.Pump();

        Assert.Equal(7, slider.Value, precision: 3);
    }

    [GtkFact]
    public void UpdatingValue_UpdatesPlatformView()
    {
        var slider = new MauiSlider { Minimum = 0, Maximum = 10 };
        var handler = CreateHandler(slider);

        slider.Value = 6;
        handler.UpdateValue(nameof(MauiSlider.Value));

        Assert.Equal(6, handler.PlatformView!.Value, precision: 3);
    }

    [GtkFact]
    public void DefaultsToZeroValue()
    {
        var slider = new MauiSlider();

        var handler = CreateHandler(slider);

        Assert.Equal(0, handler.PlatformView!.Value, precision: 3);
    }
}
