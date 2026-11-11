using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class MauiToolbar : Box
	{
		internal Button BackButton { get; }

		Label? _titleLabel;

		internal Label TitleLabel
		{
			get
			{
				if (_titleLabel != null)
				{
					return _titleLabel;
				}

				_titleLabel = new Label();
				PackStart(_titleLabel, false, false, 0);
				_titleLabel.Show();
				return _titleLabel;
			}
		}

		void RemoveTitleLabel()
		{
			if (_titleLabel is not { })
			{
				return;
			}

			Remove(_titleLabel);
			QueueResize();
			_titleLabel = default;
		}

		string _title = string.Empty;

		public string Title
		{
			get => _title;
			set
			{
				if (_title == value)
					return;

				_title = value;
				if (_title != string.Empty)
				{
					TitleLabel.Text = _title;
					QueueResize();
				}
				else
				{
					RemoveTitleLabel();
				}
			}
		}

		Image? _titleIcon;

		public Image? TitleIcon
		{
			get => _titleIcon;
			set
			{
				if (_titleIcon is { })
					Remove(_titleIcon);

				_titleIcon = value;

				if (value is { })
				{
					PackStart(value, false, false, 0);
					ReorderChild(value, 1);
					value.Show();
				}
			}
		}

		Widget? _titleView;

		public Widget? TitleView
		{
			get => _titleView;
			set
			{
				if (_titleView is { })
					Remove(_titleView);

				_titleView = value;

				if (value is { })
				{
					PackStart(value, true, true, 0);
					value.Show();
				}
			}
		}

		public Color? BarTextColor
		{
			set
			{
				if (value is { })
					TitleLabel.UpdateTextColor(value);
			}
		}

		public Color? BarBackgroundColor
		{
			set
			{
				if (value is { })
					this.SetBackgroundColor(value);
			}
		}

		public Color? IconColor
		{
			set
			{
				if (value is { })
					BackButton.SetColor(value, "color");
			}
		}

		public string? BackButtonTitle
		{
			set => BackButton.TooltipText = value;
		}

		public Box ItemsBox { get; }

		public delegate void BackButtonClickedEventHandler(object? sender);

		public event BackButtonClickedEventHandler? BackButtonClicked;

		public MauiToolbar() : base(Orientation.Horizontal, 0)
		{
			BackButton = new Button();
			BackButton.Image = new Image(Stock.GoBack, IconSize.Button);
			// Remove button border
			BackButton.Relief = ReliefStyle.None;

			BackButton.Clicked += (sender, args) => BackButtonClicked?.Invoke(sender);

			PackStart(BackButton, false, false, 0);

			ItemsBox = new Box(Orientation.Horizontal, 0);
			PackEnd(ItemsBox, false, false, 0);

			NoShowAll = true;
		}
	}
}
