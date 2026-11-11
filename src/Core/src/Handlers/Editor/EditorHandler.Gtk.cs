using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.TextView.html 

	public partial class EditorHandler : ViewHandler<IEditor, TextView>
	{

		int _maxLength = -1;

		protected override TextView CreatePlatformView()
		{
			return new() { WrapMode = WrapMode.WordChar };
		}

		protected override void ConnectHandler(TextView nativeView)
		{
			nativeView.Buffer.Changed += OnNativeTextChanged;
		}

		protected override void DisconnectHandler(TextView nativeView)
		{
			nativeView.Buffer.Changed -= OnNativeTextChanged;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (PlatformView is not { } nativeView)
				return Size.Zero;

			var res = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (res.Height != 0 && res.Width != 0)
				return res;

			var minSize = (int)Math.Round(nativeView.GetFontHeigth());

			if (res.Height == 0)
				res.Height = minSize;

			if (res.Width == 0)
				res.Width = minSize;

			return res;
		}

		protected void OnNativeTextChanged(object? sender, EventArgs e)
		{
			if (PlatformView is not { } nativeView || VirtualView is not { } virtualView)
				return;

			if (sender != nativeView.Buffer)
				return;

			var buffer = nativeView.Buffer;
			var text = buffer.Text;

			if (_maxLength >= 0 && text.Length > _maxLength)
			{
				// Gtk.TextBuffer has no insert-text cancellation flag, so the
				// text is trimmed after the change.
				buffer.Text = text[.._maxLength];
				buffer.PlaceCursor(buffer.EndIter);
				text = buffer.Text;
			}

			if (virtualView.Text != text)
				virtualView.Text = text;
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);
		}

		public static void MapFont(IEditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(editor, fontManager);
		}

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor)
		{
			if (handler.PlatformView is { } nativeView)
				nativeView.Editable = !editor.IsReadOnly;
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateTextColor(editor.TextColor);
		}

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdatePlaceholderColor(editor.PlaceholderColor);
		}

		[MissingMapper("Gtk.TextView has no letter spacing; Pango attributes are not applied to buffers")]
		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor)
		{
			// see: https://docs.gtk.org/gtk3/property.TextTag.letter-spacing.html
		}

		public static void MapMaxLength(IEditorHandler handler, IEditor editor)
		{
			if (handler is EditorHandler editorHandler)
				editorHandler._maxLength = editor.MaxLength;
		}

		[MissingMapper("GTK has no text prediction")]
		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) { }

		[MissingMapper("the keyboard is a soft-keyboard concept; GTK has no equivalent")]
		public static void MapKeyboard(IEditorHandler handler, IEditor editor) { }

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);
		}

		[MissingMapper("Gtk.TextView has no vertical text alignment")]
		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) { }

		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor)
		{
			handler.PlatformView?.UpdateCursorPosition(editor);
		}

		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor)
		{
			handler.PlatformView?.UpdateSelectionLength(editor);
		}

		/// <summary>
		/// Maps the abstract <see cref="ITextInput.IsSpellCheckEnabled"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler"> The associated handler.</param>
		/// <param name="editor"> The associated <see cref="IEditor"/> instance.</param>
		[MissingMapper("spell checking follows the system input method")]
		public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor) { }

	}

}