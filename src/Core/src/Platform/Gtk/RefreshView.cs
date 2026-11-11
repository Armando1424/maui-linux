using Gtk;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// GTK has no pull-to-refresh gesture. This container keeps the MAUI
	/// contract: it shows the content and a spinner above it while
	/// <see cref="IsRefreshing"/> is true. The refresh itself is triggered by
	/// the app (for example with a button or a scroll event), not by a gesture.
	/// </summary>
	public class RefreshView : Box
	{
		readonly Spinner _spinner;
		readonly Box _header;
		Widget? _content;

		public RefreshView() : base(Orientation.Vertical, 0)
		{
			_spinner = new Spinner { Visible = false };
			_header = new Box(Orientation.Horizontal, 6)
			{
				Halign = Align.Center,
				Valign = Align.Center,
				NoShowAll = true,
			};
			_header.PackStart(_spinner, false, false, 0);

			PackStart(_header, false, false, 0);
		}

		public bool IsRefreshing
		{
			get => _spinner.Active;
			set
			{
				_spinner.Active = value;
				_spinner.Visible = value;
				_header.Visible = value;
			}
		}

		public Spinner Spinner => _spinner;

		public Widget? Content
		{
			get => _content;
			set
			{
				if (_content is { })
					Remove(_content);

				_content = value;

				if (value is { })
				{
					PackEnd(value, true, true, 0);
					ShowAll();
					_header.Visible = IsRefreshing;
				}
			}
		}
	}
}
