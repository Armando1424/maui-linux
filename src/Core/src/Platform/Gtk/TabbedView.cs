using System;
using System.Collections.Generic;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// GTK implementation of a tabbed view. It is a <see cref="Notebook"/> whose
	/// pages come from the MAUI <c>TabbedPage</c> children.
	/// </summary>
	public class TabbedView : Notebook
	{
		readonly List<IView> _views = new();

		public TabbedView()
		{
			Scrollable = true;
			ShowTabs = true;

			SwitchPage += (_, args) =>
			{
				if ((int)args.PageNum >= _views.Count)
					return;

				ApplyTabColors();
				CurrentPageChanged?.Invoke(this, (int)args.PageNum);
			};
		}

		public event EventHandler<int>? CurrentPageChanged;

		public IReadOnlyList<IView> Views => _views;

		public void SetPages(IReadOnlyList<(IView View, Widget Widget, string? Title)> pages, IView? current)
		{
			foreach (var child in Children)
				Remove(child);

			_views.Clear();

			foreach (var page in pages)
			{
				var label = new Label(page.Title ?? string.Empty);
				AppendPage(page.Widget, label);
				_views.Add(page.View);
			}

			ShowAll();

			if (current is { })
				SetCurrent(current);
			else if (_views.Count > 0)
				CurrentPage = 0;

			ApplyTabColors();
		}

		public void SetCurrent(IView view)
		{
			var index = IndexOf(view);
			if (index >= 0 && CurrentPage != index)
				CurrentPage = index;
		}

		public int IndexOf(IView view)
		{
			for (var i = 0; i < _views.Count; i++)
			{
				if (ReferenceEquals(_views[i], view))
					return i;
			}

			return -1;
		}

		Color? _selectedTabColor;
		Color? _unselectedTabColor;

		public void UpdateTabColors(Color? selected, Color? unselected)
		{
			_selectedTabColor = selected;
			_unselectedTabColor = unselected;
			ApplyTabColors();
		}

		public void UpdateBarColors(Color? background, Color? text)
		{
			if (background is { })
				this.SetStyleColor(background, "notebook", "background-color", "header");

			if (text is { })
			{
				for (var i = 0; i < _views.Count; i++)
					TabLabel(i)?.UpdateTextColor(text);
			}
		}

		void ApplyTabColors()
		{
			for (var i = 0; i < _views.Count; i++)
			{
				var label = TabLabel(i);
				if (label is null)
					continue;

				var color = i == CurrentPage ? _selectedTabColor : _unselectedTabColor;
				if (color is { })
					label.UpdateTextColor(color);
			}
		}

		Label? TabLabel(int index)
		{
			var child = GetNthPage(index);
			return GetTabLabel(child) as Label;
		}
	}
}
