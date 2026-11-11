using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class ListViewHandler : ViewHandler<ListView, MauiListView>
	{
		public static IPropertyMapper<ListView, ListViewHandler> Mapper = new PropertyMapper<ListView, ListViewHandler>(
			ViewRenderer.VisualElementRendererMapper)
		{
			[VisualElement.BackgroundProperty.PropertyName] = MapBackground,
			[ListView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[ListView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[ListView.SelectedItemProperty.PropertyName] = MapSelectedItem,
			[ListView.RowHeightProperty.PropertyName] = MapRowHeight,
		};

		public static CommandMapper<ListView, ListViewHandler> CommandMapper = new(ViewRenderer.VisualElementRendererCommandMapper) { };

		public ListViewHandler() : base(Mapper, CommandMapper) { }

		public ListViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		public ListViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		protected override MauiListView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ListView)}");

			return new MauiListView();
		}

		protected override void ConnectHandler(MauiListView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.SelectedIndexChanged += OnSelectedIndexChanged;
			UpdateRows();
		}

		protected override void DisconnectHandler(MauiListView nativeView)
		{
			nativeView.SelectedIndexChanged -= OnSelectedIndexChanged;
			base.DisconnectHandler(nativeView);
		}

		void OnSelectedIndexChanged(object? sender, int index)
		{
			var items = GetItems();

			if (index >= 0 && index < items.Count)
				VirtualView.SelectedItem = items[index];
		}

		IReadOnlyList<object> GetItems()
		{
			if (VirtualView?.ItemsSource is not IEnumerable source)
				return Array.Empty<object>();

			return source.Cast<object>().ToList();
		}

		void UpdateRows()
		{
			if (PlatformView is not { } platformView || MauiContext is not { } context || VirtualView is not { } listView)
				return;

			var rows = new List<Widget>();

			foreach (var item in GetItems())
				rows.Add(CellRowFactory.Create(item, listView.ItemTemplate, context));

			var selectedIndex = SelectedIndex();

			platformView.SetRows(rows, selectedIndex);
		}

		int SelectedIndex()
		{
			var items = GetItems();

			for (var i = 0; i < items.Count; i++)
			{
				if (ReferenceEquals(items[i], VirtualView.SelectedItem) || Equals(items[i], VirtualView.SelectedItem))
					return i;
			}

			return -1;
		}

		public static void MapItemsSource(ListViewHandler handler, ListView listView) => handler.UpdateRows();

		public static void MapItemTemplate(ListViewHandler handler, ListView listView) => handler.UpdateRows();

		[MissingMapper("Gtk.ListBox does not expose a per-row height; it follows the row content")]
		public static void MapRowHeight(ListViewHandler handler, ListView listView) { }

		public static void MapSelectedItem(ListViewHandler handler, ListView listView)
		{
			if (handler.PlatformView is { } platformView)
				platformView.Select(handler.SelectedIndex());
		}
	}

	/// <summary>
	/// Renders a MAUI <see cref="Cell"/> (or a plain item) as a GTK row for the
	/// compatibility ListView and TableView.
	/// </summary>
	internal static class CellRowFactory
	{
		public static Widget Create(object? item, DataTemplate? template, IMauiContext context)
		{
			if (template is { })
			{
				var content = template.CreateContent();

				switch (content)
				{
					case ViewCell viewCell when viewCell.View is { } view:
						return view.ToPlatform(context);

					case TextCell textCell:
						return TextRow(textCell.Text, textCell.Detail);
				}
			}

			return new Gtk.Label(item?.ToString() ?? string.Empty)
			{
				Halign = Align.Start,
				Margin = 6,
			};
		}

		public static Widget CreateCell(Cell cell, IMauiContext context)
		{
			switch (cell)
			{
				case ViewCell viewCell when viewCell.View is { } view:
					return view.ToPlatform(context);

				case TextCell textCell:
					return TextRow(textCell.Text, textCell.Detail);

				case SwitchCell switchCell:
					return SwitchRow(switchCell);

				case EntryCell entryCell:
					return EntryRow(entryCell);

				default:
					return new Gtk.Label(cell.ToString() ?? string.Empty) { Halign = Align.Start, Margin = 6 };
			}
		}

		static Widget TextRow(string? text, string? detail)
		{
			var box = new Gtk.Box(Orientation.Vertical, 2) { Margin = 6 };
			box.PackStart(new Gtk.Label(text ?? string.Empty)
			{
				Halign = Align.Start,
				Xalign = 0,
			}, false, false, 0);

			if (!string.IsNullOrEmpty(detail))
			{
				box.PackStart(new Gtk.Label(detail)
				{
					Halign = Align.Start,
					Xalign = 0,
				}, false, false, 0);
			}

			return box;
		}

		static Widget SwitchRow(SwitchCell cell)
		{
			var box = new Gtk.Box(Orientation.Horizontal, 6) { Margin = 6 };
			box.PackStart(new Gtk.Label(cell.Text ?? string.Empty) { Halign = Align.Start, Xalign = 0 }, true, true, 0);

			var toggle = new Gtk.Switch { Active = cell.On, Valign = Align.Center };
			toggle.StateSet += (_, args) =>
			{
				cell.On = args.State;
				args.RetVal = false;
			};

			box.PackEnd(toggle, false, false, 0);
			return box;
		}

		static Widget EntryRow(EntryCell cell)
		{
			var box = new Gtk.Box(Orientation.Vertical, 2) { Margin = 6 };

			if (!string.IsNullOrEmpty(cell.Label))
				box.PackStart(new Gtk.Label(cell.Label) { Halign = Align.Start, Xalign = 0 }, false, false, 0);

			var entry = new Gtk.Entry
			{
				Text = cell.Text ?? string.Empty,
				PlaceholderText = cell.Placeholder ?? string.Empty,
			};

			entry.Changed += (_, _) => cell.Text = entry.Text;
			entry.Activated += (_, _) => cell.SendCompleted();

			box.PackStart(entry, false, false, 0);
			return box;
		}
	}
}
