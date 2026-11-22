using System.Threading;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Item de menú de GTK4. No es un widget: envuelve un GLib.MenuItem y, cuando
/// se pulsa, dispara <see cref="Activated"/> a través de un GLib.SimpleAction
/// registrado en la aplicación por defecto.
/// </summary>
public class MauiMenuItem
{
	static int _sequence;

	string _text = string.Empty;
	bool _enabled = true;
	MauiMenu? _submenu;

	public event System.EventHandler? Activated;

	public bool IsSeparator { get; init; }

	public MauiMenu? Submenu => _submenu;

	public MauiMenu EnsureSubMenu => _submenu ??= new MauiMenu();

	public void AppendSubItem(MauiMenuItem subItem) => EnsureSubMenu.Append(subItem);

	public void RemoveSubItem(MauiMenuItem subItem) => _submenu?.Remove(subItem);

	public void InsertSubItem(MauiMenuItem subItem, int index) => EnsureSubMenu.Insert(subItem, index);

	public void ClearSubItems() => _submenu?.Clear();

	public void UpdateText(string text)
	{
		_text = text ?? string.Empty;
		_submenu?.Invalidate();
	}

	public void UpdateIsEnabled(bool enabled) => _enabled = enabled;

	// GTK4 no oculta/muestra items de un GMenu: se reconstruye el modelo.
	public void Show()
	{
	}

	internal GLib.MenuItem ToMenuItem()
	{
		if (IsSeparator)
			return new GLib.MenuItem("─────", (string?)null);

		if (_submenu is not null)
			return GLib.MenuItem.NewSubmenu(_text, _submenu.Model);

		var actionName = "maui" + Interlocked.Increment(ref _sequence);
		var action = new GLib.SimpleAction(actionName, null);

		if (_enabled)
			action.Activated += (_, _) => Activated?.Invoke(this, System.EventArgs.Empty);

		GLib.Application.Default?.AddAction(action);

		return new GLib.MenuItem(_text, "app." + actionName);
	}
}
