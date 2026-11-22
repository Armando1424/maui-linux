using System.Collections.Generic;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Contenedor de items de menú en GTK4: tanto la barra (PopoverMenuBar) como un
/// submenú (GLib.Menu) exponen la misma API para que los handlers no sepan cuál es.
/// </summary>
public interface IMauiMenuContainer
{
	IReadOnlyList<MauiMenuItem> Children { get; }
	void Append(MauiMenuItem item);
	void Insert(MauiMenuItem item, int index);
	void Remove(MauiMenuItem item);
	void Clear();
}
