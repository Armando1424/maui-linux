using System;
using System.Collections.Generic;
using Gtk;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Minimal GTK list used by the compatibility <c>ListView</c>. A ListBox is
	/// enough for the supported feature set: item rows, single selection and a
	/// selected index that is kept in sync with the virtual view.
	/// </summary>
	public class MauiListView : ScrolledWindow
	{
		readonly ListBox _list = new();

		public MauiListView()
		{
			Add(_list);
			_list.SelectionMode = SelectionMode.Single;
			_list.RowSelected += (_, args) =>
			{
				if (args.Row is { })
					SelectedIndexChanged?.Invoke(this, args.Row.Index);
			};
		}

		public event EventHandler<int>? SelectedIndexChanged;

		readonly System.Collections.Generic.List<Widget> _rows = new();

		public IReadOnlyList<Widget> Rows => _rows;

		public void SetRows(IEnumerable<Widget> rows, int selectedIndex)
		{
			foreach (var child in _rows)
				_list.Remove(child);

			_rows.Clear();

			foreach (var row in rows)
				_list.Append(row);

			ShowAll();
			Select(selectedIndex);
		}

		public void Select(int index)
		{
			var row = index >= 0 ? _list.GetRowAtIndex(index) : null;

			if (row is { })
				_list.SelectRow(row);
			else
				_list.UnselectAll();
		}
	}
}
