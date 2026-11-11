using System;
using Xunit;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;

/// <summary>
/// GTK is single threaded: every GTK based test runs in this serialized
/// collection and GTK is initialized once by <see cref="GtkFixture"/>.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GtkCollection : ICollectionFixture<GtkFixture>
{
    public const string Name = "Gtk";
}

public sealed class GtkFixture
{
    public GtkFixture()
    {
        var args = Array.Empty<string>();

        if (!global::Gtk.Application.InitCheck("Microsoft.Maui.Controls.Gtk.UnitTests", ref args))
            throw new InvalidOperationException("GTK could not be initialized: no display available.");
    }
}
