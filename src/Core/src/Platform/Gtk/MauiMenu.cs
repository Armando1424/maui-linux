using System.Collections.Generic;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Menú de GTK4. GTK3 usaba un árbol de widgets (GtkMenu/MenuShell); GTK4
/// trabaja con un modelo de datos (GLib.Menu) que se entrega a un
/// Gtk.PopoverMenuBar. Aquí se guarda la lista de items y se reconstruye el
/// modelo cada vez que cambia, para que el texto y el estado queden al día.
/// </summary>
public class MauiMenu : IMauiMenuContainer
{
	readonly List<MauiMenuItem> _items = new();

	internal GLib.Menu Model { get; }

	public MauiMenu() : this(new GLib.Menu())
	{
	}

	internal MauiMenu(GLib.Menu model) => Model = model;

	public IReadOnlyList<MauiMenuItem> Children => _items;

	void Rebuild()
	{
		Model.RemoveAll();
		foreach (var item in _items)
			Model.AppendItem(item.ToMenuItem());
	}

	public void Append(MauiMenuItem item)
	{
		_items.Add(item);
		Rebuild();
	}

	public void Insert(MauiMenuItem item, int index)
	{
		index = System.Math.Clamp(index, 0, _items.Count);
		_items.Insert(index, item);
		Rebuild();
	}

	public void Remove(MauiMenuItem item)
	{
		if (_items.Remove(item))
			Rebuild();
	}

	public void Clear()
	{
		_items.Clear();
		Rebuild();
	}

	internal void Invalidate() => Rebuild();
}
