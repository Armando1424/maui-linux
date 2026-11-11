using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;
using Xunit;
using MauiProgressBar = Microsoft.Maui.Controls.ProgressBar;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.HandlerTests;

[Collection(GtkCollection.Name)]
public sealed class ProgressBarHandlerTests
{
    private static ProgressBarHandler CreateHandler(MauiProgressBar progressBar)
    {
        var handler = new ProgressBarHandler();
        handler.SetMauiContext(HandlerTestContext.Context);
        handler.SetVirtualView(progressBar);
        return handler;
    }

    [GtkFact]
    public void MapsProgress()
    {
        var progressBar = new MauiProgressBar { Progress = 0.25 };

        var handler = CreateHandler(progressBar);

        Assert.Equal(0.25, handler.PlatformView!.Fraction, precision: 3);
    }

    [GtkFact]
    public void MapsPercentTooltip()
    {
        var progressBar = new MauiProgressBar { Progress = 0.5 };

        var handler = CreateHandler(progressBar);

        Assert.Equal("50%", handler.PlatformView!.TooltipText);
    }

    [GtkFact]
    public void UpdatingProgress_UpdatesPlatformView()
    {
        var progressBar = new MauiProgressBar();
        var handler = CreateHandler(progressBar);

        progressBar.Progress = 0.75;
        handler.UpdateValue(nameof(MauiProgressBar.Progress));

        Assert.Equal(0.75, handler.PlatformView!.Fraction, precision: 3);
    }
}
