using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class DateEventArgs(DateTime date) : EventArgs
	{
		public DateTime Date { get; } = date;
	}

	/// <summary>
	/// GTK4 eliminó los grabs de Gdk.Seat: el foco lo gestiona la propia ventana
	/// o el popover, así que aquí solo queda el foco.
	/// </summary>
	public class GrabHelper
	{
		public static void GrabWindow(Window window) => window.GrabFocus();

		public static void RemoveGrab(Window window)
		{
		}
	}

	public partial class DatePickerWindow : Window
	{
		readonly RangeCalendar _calendar;

		public delegate void DateEventHandler(object sender, DateEventArgs args);

		public event DateEventHandler? OnDateTimeChanged;

		public DatePickerWindow(DateTime initialDate, Window? parentWindow)
			: base()
		{
			if (parentWindow is not null)
				TransientFor = parentWindow;

			Title = "DatePicker";
			Resizable = false;
			Decorated = false;

			var datebox = new Box(Orientation.Vertical, 6);
			datebox.MarginTop = datebox.MarginBottom = 6;
			datebox.MarginStart = datebox.MarginEnd = 6;

			_calendar = new RangeCalendar();
			_calendar.CanFocus = true;
			_calendar.DaySelected += OnCalendarDaySelected;

			datebox.Append(_calendar);
			Child = datebox;

			SelectedDate = initialDate;
			Show();
		}

		public DateTime SelectedDate
		{
			get => _calendar.Date;
			set => _calendar.Date = value;
		}

		public DateTime MinimumDate
		{
			get => _calendar.MinimumDate;
			set => _calendar.MinimumDate = value;
		}

		public DateTime MaximumDate
		{
			get => _calendar.MaximumDate;
			set => _calendar.MaximumDate = value;
		}

		protected virtual void OnCalendarDaySelected(object? sender, EventArgs eventArgs)
		{
			OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
		}

		public void ShowCalendar()
		{
			Show();
			GrabHelper.GrabWindow(this);
		}

		void CloseCalendar()
		{
			GrabHelper.RemoveGrab(this);
			Hide();
		}

		/// <summary>
		/// Gtk.Calendar de GTK4: su Date es un GLib.DateTime y solo existe la
		/// señal day-selected (el doble clic desapareció).
		/// </summary>
		class RangeCalendar : Calendar
		{
			DateTime _minimumDate = new(1900, 1, 1);
			DateTime _maximumDate = new(2199, 1, 1);

			public DateTime MinimumDate
			{
				get => _minimumDate;
				set
				{
					if (MaximumDate < value)
						throw new InvalidOperationException($"{nameof(MinimumDate)} must be lower than {nameof(MaximumDate)}");

					_minimumDate = value;
				}
			}

			public DateTime MaximumDate
			{
				get => _maximumDate;
				set
				{
					if (MinimumDate > value)
						throw new InvalidOperationException($"{nameof(MaximumDate)} must be greater than {nameof(MinimumDate)}");

					_maximumDate = value;
				}
			}

			public new DateTime Date
			{
				get
				{
					var date = base.Date;
					return new DateTime(date.Year, date.Month, date.DayOfMonth);
				}
				set => base.Date = new GLib.DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
			}

			protected override void OnDaySelected()
			{
				if (Date < MinimumDate)
					Date = MinimumDate;
				else if (Date > MaximumDate)
					Date = MaximumDate;

				base.OnDaySelected();
			}
		}
	}

	public class CustomComboBox : Box
	{
		Color _color = Colors.Black;

		public CustomComboBox() : base(Orientation.Horizontal, 0)
		{
			Entry = new Entry { IsEditable = true };

			// GTK4 quitó los iconos de Entry: el botón va como hermano.
			ToggleButton = new Button { IconName = "pan-down-symbolic" };
			ToggleButton.AddCssClass("flat");

			Append(Entry);
			Append(ToggleButton);
		}

		public Entry Entry { get; }

		public Button ToggleButton { get; }

		public Color Color
		{
			get => _color;
			set
			{
				_color = value;
				Entry.UpdateTextColor(_color);
			}
		}
	}

	public class MauiDatePicker : EventBox
	{
		readonly CustomComboBox _comboBox;
		DateTime _currentDate;
		string? _dateFormat;
		DatePickerWindow? _pickerWindow;

		public event EventHandler DateChanged = new((sender, args) => { });
		public event EventHandler GotFocus = new((sender, args) => { });
		public event EventHandler LostFocus = new((sender, args) => { });

		public MauiDatePicker()
		{
			_comboBox = new CustomComboBox();
			Child = _comboBox;
			Show();

			Date = DateTime.Now;
			Format = string.Empty;
			MinDate = new DateTime(1900, 1, 1);
			MaxDate = new DateTime(2199, 1, 1);

			// El calendario se abre al pulsar el botón; GTK4 ya no tiene
			// icon-press ni focus-grabbed en Entry.
			_comboBox.ToggleButton.Clicked += OnBtnShowCalendarClicked;
		}

		public DateTime Date
		{
			get => _currentDate;
			set
			{
				_currentDate = value;
				UpdateEntryText();
			}
		}

		public DateTime MinDate { get; set; }

		public DateTime MaxDate { get; set; }

		public Entry Entry => _comboBox.Entry;

		public string? Format
		{
			get => _dateFormat;
			set
			{
				_dateFormat = value;
				UpdateEntryText();
			}
		}

		void ShowPickerWindow()
		{
			if (_pickerWindow is not { })
			{
				var picker = new DatePickerWindow(Date, Toplevel as Window);
				picker.MinimumDate = MinDate;
				picker.MaximumDate = MaxDate;
				picker.OnDateTimeChanged += OnPopupDateChanged;
				picker.CloseRequest += OnPickerClosed;
				_pickerWindow = picker;
			}
			else
			{
				if (_pickerWindow.SelectedDate != Date)
					_pickerWindow.SelectedDate = Date;

				_pickerWindow.MinimumDate = MinDate;
				_pickerWindow.MaximumDate = MaxDate;
				_pickerWindow.ShowCalendar();
			}
		}

		void OnPopupDateChanged(object sender, DateEventArgs args)
		{
			var date = args.Date;

			if (date < MinDate)
			{
				Date = MinDate;
				return;
			}

			if (date > MaxDate)
			{
				Date = MaxDate;
				return;
			}

			Date = args.Date;
			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		void UpdateEntryText()
		{
			_comboBox.Entry.Text = _currentDate.ToString(string.IsNullOrEmpty(_dateFormat) ? "D" : _dateFormat);
		}

		void OnBtnShowCalendarClicked(object? sender, EventArgs eventArgs)
		{
			ShowPickerWindow();
			GotFocus?.Invoke(this, EventArgs.Empty);
		}

		void OnPickerClosed(object? sender, Gtk.CloseRequestArgs eventArgs)
		{
			if (sender is not DatePickerWindow window)
				return;

			window.Hide();
			LostFocus?.Invoke(this, EventArgs.Empty);
		}
	}
}
