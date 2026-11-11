using System;
using System.Collections.Generic;
using Gtk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class TableViewHandler : ViewHandler<TableView, MauiTableView>
	{
		public static IPropertyMapper<TableView, TableViewHandler> Mapper = new PropertyMapper<TableView, TableViewHandler>(
			ViewRenderer.VisualElementRendererMapper)
		{
			[VisualElement.BackgroundProperty.PropertyName] = MapBackground,
			[TableView.RowHeightProperty.PropertyName] = MapRowHeight,
		};

		public static CommandMapper<TableView, TableViewHandler> CommandMapper = new(ViewRenderer.VisualElementRendererCommandMapper) { };

		public TableViewHandler() : base(Mapper, CommandMapper) { }

		public TableViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		public TableViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		protected override MauiTableView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(TableView)}");

			return new MauiTableView();
		}

		protected override void ConnectHandler(MauiTableView nativeView)
		{
			base.ConnectHandler(nativeView);
			UpdateSections();
		}

		void UpdateSections()
		{
			if (PlatformView is not { } platformView || MauiContext is not { } context || VirtualView is not { } table)
				return;

			var sections = new List<Widget>();

			foreach (var section in table.Root)
			{
				var box = new Gtk.Box(Orientation.Vertical, 0);

				if (!string.IsNullOrEmpty(section.Title))
				{
					box.PackStart(new Gtk.Label(section.Title)
					{
						Halign = Align.Start,
						Xalign = 0,
						Margin = 6,
					}, false, false, 0);
				}

				foreach (var cell in section)
					box.PackStart(CellRowFactory.CreateCell(cell, context), false, false, 0);

				sections.Add(box);
			}

			platformView.SetSections(sections);
		}

		[MissingMapper("Gtk rows follow their content; there is no per-row height")]
		public static void MapRowHeight(TableViewHandler handler, IView view)
		{
			handler.UpdateSections();
		}
	}
}
