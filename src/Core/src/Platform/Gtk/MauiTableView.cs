using System.Collections.Generic;
using Gtk;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Minimal GTK table used by the compatibility <c>TableView</c>: a vertical
	/// box with one block per section and one row per cell.
	/// </summary>
	public class MauiTableView : ScrolledWindow
	{
		readonly Box _content = new(Orientation.Vertical, 0);

		public MauiTableView()
		{
			Add(_content);
		}

		public void SetSections(IEnumerable<Widget> sections)
		{
			foreach (var child in _content.Children)
				_content.Remove(child);

			foreach (var section in sections)
				_content.PackStart(section, false, false, 0);

			ShowAll();
		}
	}
}
