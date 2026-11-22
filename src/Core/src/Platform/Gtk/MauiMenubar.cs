using System.Collections.Generic;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Barra de menú de GTK4: un Gtk.PopoverMenuBar sobre un GLib.Menu.
/// Sustituye a Gtk.MenuBar, que GTK4 eliminó.
/// </summary>
public class MauiMenuBar : Gtk.PopoverMenuBar, IMauiMenuContainer
{
	readonly MauiMenu _menu;

	public MauiMenuBar() : base(new GLib.Menu())
	{
		_menu = new MauiMenu((GLib.Menu)MenuModel!);
	}

	public IReadOnlyList<MauiMenuItem> Children => _menu.Children;

	public void Append(MauiMenuItem item) => _menu.Append(item);

	public void Insert(MauiMenuItem item, int index) => _menu.Insert(item, index);

	public void Remove(MauiMenuItem item) => _menu.Remove(item);

	public void Clear() => _menu.Clear();
}
