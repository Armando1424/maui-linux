using System;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : Gtk.Label
	{

		public MauiTimePicker() : base()
		{
			Format = string.Empty;
		}

		TimeSpan _time;

		public TimeSpan Time
		{
			get => _time;
			set
			{
				_time = value;
				UpdateText();
			}
		}

		string _format = string.Empty;

		public string Format
		{
			get => _format;
			set
			{
				_format = value ?? string.Empty;
				UpdateText();
			}
		}

		void UpdateText() =>
			Text = string.IsNullOrEmpty(_format)
				? _time.ToString()
				: DateTime.Today.Add(_time).ToString(_format);

	}
}