using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		const string PlaceholderKey = "maui-editor-placeholder";

		public static void UpdateText(this TextView platformView, IEditor editor)
		{
			var text = editor.Text;
			var buffer = platformView.Buffer;

			if (buffer.Text == text)
				return;

			if (text == null)
			{
				buffer.Clear();
			}

			else
			{
				buffer.Text = text;
			}
		}

		public static void UpdatePlaceholder(this TextView platformView, IEditor editor)
		{
			var label = platformView.Data[PlaceholderKey] as Label;

			if (label is null)
			{
				label = new Label
				{
					Halign = Align.Start,
					Valign = Align.Start,
					Xalign = 0,
					Yalign = 0,
					MarginStart = 4,
					MarginTop = 2,
					NoShowAll = true,
				};

				platformView.Add(label);
				platformView.Data[PlaceholderKey] = label;

				// The placeholder is only a visual hint; let the clicks go to
				// the text view so it keeps the focus.
				label.AddEvents((int)Gdk.EventMask.ButtonPressMask);

				platformView.Buffer.Changed += (_, _) => UpdatePlaceholderVisibility(platformView);
			}

			label.Text = editor.Placeholder ?? string.Empty;
			UpdatePlaceholderVisibility(platformView);
		}

		public static void UpdatePlaceholderColor(this TextView platformView, Graphics.Color? color)
		{
			if (platformView.Data[PlaceholderKey] is Label label && color is { })
				label.UpdateTextColor(color);
		}

		public static void UpdateHorizontalTextAlignment(this TextView platformView, IEditor editor)
		{
			platformView.Justification = editor.HorizontalTextAlignment.ToJustification();
		}

		public static void UpdateCursorPosition(this TextView platformView, ITextInput editor)
		{
			var buffer = platformView.Buffer;
			var position = global::System.Math.Clamp(editor.CursorPosition, 0, buffer.CharCount);

			buffer.PlaceCursor(buffer.GetIterAtOffset(position));
		}

		public static void UpdateSelectionLength(this TextView platformView, ITextInput editor)
		{
			var buffer = platformView.Buffer;
			var start = global::System.Math.Clamp(editor.CursorPosition, 0, buffer.CharCount);
			var end = global::System.Math.Clamp(start + editor.SelectionLength, 0, buffer.CharCount);

			buffer.SelectRange(buffer.GetIterAtOffset(start), buffer.GetIterAtOffset(end));
		}

		static void UpdatePlaceholderVisibility(TextView platformView)
		{
			if (platformView.Data[PlaceholderKey] is not Label label)
				return;

			label.Visible = platformView.Buffer.CharCount == 0 && !string.IsNullOrEmpty(label.Text);
		}
	}
}
