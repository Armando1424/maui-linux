using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform;

/// <summary>
/// Minimal GTK Shell: a flyout with the ShellItems, a tab bar with the
/// ShellSections of the current item and a content area that hosts the
/// section's <see cref="Microsoft.Maui.Platform.NavigationView"/>.
/// </summary>
public class ShellView : Gtk.Box
{
	readonly Gtk.Box _flyoutItems;
	readonly Gtk.ScrolledWindow _flyoutScroll;
	readonly Gtk.Box _content;
	readonly Gtk.Box _topBar;
	readonly Gtk.Box _sectionTabs;
	readonly Gtk.Button _menuButton;
	Gtk.Widget? _sectionView;

	Shell? _shell;
	ShellItem? _currentItem;
	ShellSection? _currentSection;
	bool _isOpened;

	public ShellView() : base(Orientation.Horizontal, 0)
	{
		_flyoutItems = new Gtk.Box(Orientation.Vertical, 0);
		_flyoutScroll = new Gtk.ScrolledWindow
		{
			Child = _flyoutItems,
			WidthRequest = 220,
			NoShowAll = true,
		};

		_menuButton = new Gtk.Button
		{
			Image = new Gtk.Image { IconName = "open-menu-symbolic", PixelSize = 16 },
			Relief = ReliefStyle.None,
			NoShowAll = true,
		};
		_menuButton.Clicked += (_, _) =>
		{
			IsOpened = !IsOpened;
		};

		_topBar = new Gtk.Box(Orientation.Horizontal, 0);
		_topBar.PackStart(_menuButton, false, false, 0);

		_sectionTabs = new Gtk.Box(Orientation.Horizontal, 0);
		_topBar.PackStart(_sectionTabs, true, true, 0);

		_content = new Gtk.Box(Orientation.Vertical, 0);
		_content.PackStart(_topBar, false, false, 0);

		PackStart(_flyoutScroll, false, false, 0);
		PackEnd(_content, true, true, 0);
	}

	public IMauiContext? MauiContext { get; set; }

	public bool IsOpened
	{
		get => _isOpened;
		set
		{
			_isOpened = value;
			UpdateFlyoutVisibility();
		}
	}

	FlyoutBehavior _behavior = FlyoutBehavior.Flyout;

	public void UpdateFlyoutBehavior(FlyoutBehavior behavior)
	{
		_behavior = behavior;
		_menuButton.Visible = behavior == FlyoutBehavior.Flyout;
		UpdateFlyoutVisibility();
	}

	public void UpdateDrawerWidth(double width)
	{
		if (width > 0)
			_flyoutScroll.WidthRequest = (int)width;
	}

	public void UpdateBackgroundColor(Color? color)
	{
		if (color is { })
			this.SetBackgroundColor(color);
	}

	public void UpdateCurrentItem(ShellItem? item)
	{
		_currentItem = item;

		foreach (var child in _sectionTabs.Children)
			_sectionTabs.Remove(child);

		if (item is { })
		{
			foreach (var section in item.Items)
			{
				var button = new Gtk.Button(section.Title ?? string.Empty)
				{
					Relief = ReliefStyle.None,
				};

				button.Clicked += (_, _) =>
				{
					item.CurrentItem = section;
					ShowSection(section);
				};

				_sectionTabs.PackStart(button, false, false, 0);
				button.Show();
			}
		}

		ShowSection(item?.CurrentItem);
	}

	public void UpdateItems()
	{
		if (_shell is null)
			return;

		foreach (var child in _flyoutItems.Children)
			_flyoutItems.Remove(child);

		foreach (var item in _shell.Items)
		{
			var button = new Gtk.Button(item.Title ?? string.Empty)
			{
				Relief = ReliefStyle.None,
				Halign = Align.Fill,
			};

			button.Clicked += (_, _) =>
			{
				_shell.CurrentItem = item;
				IsOpened = false;
			};

			_flyoutItems.PackStart(button, false, false, 0);
			button.Show();
		}

		_flyoutItems.ShowAll();
	}

	public void SetShell(Shell shell)
	{
		_shell = shell;
		UpdateItems();
	}

	void ShowSection(ShellSection? section)
	{
		if (MauiContext is not { } context)
			return;

		if (_sectionView is { })
		{
			_content.Remove(_sectionView);
			_sectionView = null;
		}

		_currentSection = section;

		if (section is null)
			return;

		_sectionView = section.ToPlatform(context);
		_content.PackEnd(_sectionView, true, true, 0);
		_sectionView.ShowAll();
	}

	void UpdateFlyoutVisibility()
	{
		var visible = _behavior switch
		{
			FlyoutBehavior.Disabled => false,
			FlyoutBehavior.Locked => true,
			_ => _isOpened,
		};

		_flyoutScroll.Visible = visible;
	}

	[MissingMapper("the flyout backdrop is not drawn on GTK")]
	public void UpdateFlyoutBackDrop(Brush? backdrop) { }

	[MissingMapper("flyout header and footer templates are not implemented on GTK")]
	public void UpdateFlyoutFooter(Shell? shell) { }

	[MissingMapper("flyout header and footer templates are not implemented on GTK")]
	public void UpdateFlyoutHeader(Shell? shell) { }

	[MissingMapper("a custom flyout view is not implemented on GTK")]
	public void UpdateFlyout(IView? flyout) { }
}
