using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Maui.Platform;

/// <summary>
/// GTK4 eliminó los proveedores de estilo por widget: ahora hay un
/// Gtk.CssProvider por display. Para conservar la API de MAUI (que asigna
/// propiedades CSS a un widget concreto), a cada widget se le da una clase
/// única y se mantiene una hoja global que se reconstruye al cambiar.
/// </summary>
internal static class MauiCss
{
	const uint Priority = 800;

	static Gtk.CssProvider? _provider;
	static int _sequence;

	static readonly Dictionary<Gtk.Widget, Dictionary<string, string>> _widgets = new();
	static readonly StringBuilder _sheet = new();

	static void EnsureProvider()
	{
		if (_provider is not null)
			return;

		_provider = new Gtk.CssProvider();
		if (Gdk.Display.Default is { } display)
			Gtk.StyleContext.AddProviderForDisplay(display, _provider, Priority);
	}

	static string ClassFor(Gtk.Widget widget, Dictionary<string, string> props)
	{
		if (props.TryGetValue("--class", out var cls))
			return cls;

		cls = "maui-" + Interlocked.Increment(ref _sequence);
		props["--class"] = cls;
		widget.AddCssClass(cls);

		return cls;
	}

	static void Rebuild()
	{
		_sheet.Clear();

		foreach (var (_, props) in _widgets)
		{
			var cls = props["--class"];
			_sheet.Append('.').Append(cls).Append(" { ");

			foreach (var (name, value) in props)
			{
				if (name == "--class")
					continue;

				_sheet.Append(name).Append(": ").Append(value).Append(';');
			}

			_sheet.Append(" }\n");
		}

		_provider?.LoadFromData(_sheet.ToString());
	}

	/// <summary>Fija una propiedad CSS en un widget concreto.</summary>
	public static void Set(Gtk.Widget widget, string name, string value)
	{
		if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
			return;

		EnsureProvider();

		if (!_widgets.TryGetValue(widget, out var props))
		{
			props = new Dictionary<string, string>();
			_widgets[widget] = props;
		}

		ClassFor(widget, props);
		props[name] = value;

		Rebuild();
	}

	/// <summary>Aplica familia, tamaño, peso y estilo de fuente.</summary>
	public static void SetFont(Gtk.Widget widget, string? family, double size, bool bold, bool italic)
	{
		if (!string.IsNullOrWhiteSpace(family))
			Set(widget, "font-family", $"\"{family}\"");

		if (size > 0)
			Set(widget, "font-size", $"{size.ToString(System.Globalization.CultureInfo.InvariantCulture)}pt");

		if (bold)
			Set(widget, "font-weight", "bold");

		if (italic)
			Set(widget, "font-style", "italic");
	}
}
