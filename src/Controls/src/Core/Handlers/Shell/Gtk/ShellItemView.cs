using Gtk;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Platform element for a ShellItem. The GTK shell draws the flyout and the
	/// section tabs, so this view only keeps the item title and appearance
	/// values that the shell may read.
	/// </summary>
	public class ShellItemView : Box
	{
		readonly Gtk.Label _title;

		public ShellItemView(object view) : base(Orientation.Vertical, 0)
		{
			_title = new Gtk.Label();
			PackStart(_title, false, false, 0);
		}

		public string Title
		{
			get => _title.Text;
			set => _title.Text = value ?? string.Empty;
		}

		[MissingMapper("the GTK shell draws its own tab bar; the item view is not shown")]
		public void UpdateTabBar(bool visible) { }

		[MissingMapper("the GTK shell switches sections through ShellItem.CurrentItem")]
		public void UpdateCurrentItem(object? current) { }

		public void UpdateBottomTabBarColors(Color? background, Color? title, Color? unselected)
		{
			if (title is { })
				_title.UpdateTextColor(title);

			if (background is { })
				this.SetBackgroundColor(background);
		}
	}
}
