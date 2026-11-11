using System;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class SearchBarHandler : ViewHandler<ISearchBar, MauiSearchBar>
	{

		protected override MauiSearchBar CreatePlatformView()
		{
			return new MauiSearchBar();
		}

		protected override void ConnectHandler(MauiSearchBar nativeView)
		{
			nativeView.Entry.Changed += OnNativeViewChanged;
		}

		protected override void DisconnectHandler(MauiSearchBar nativeView)
		{
			nativeView.Entry.Changed -= OnNativeViewChanged;
		}

		protected void OnNativeViewChanged(object? sender, EventArgs e)
		{
			if (sender != PlatformView)
				return;

			PlatformView?.Entry.OnTextChanged(VirtualView);
		}

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.Entry.UpdateText(searchBar);
		}

		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.Entry.UpdatePlaceholder(searchBar);

		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.Entry.UpdateIsReadOnly(searchBar);

		}

		public Gtk.Entry? QueryEditor => PlatformView?.Entry;

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler.PlatformView?.Entry is { } nativeView)
				nativeView.Alignment = searchBar.HorizontalTextAlignment.ToXyAlign();
		}

		[MissingMapper("GTK's Entry has no vertical text alignment")]
		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.Entry?.UpdateTextColor(searchBar.TextColor);

		}

		[MissingMapper("GTK renders the placeholder with the theme color")]
		public static void MapPlaceholderColor(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler.PlatformView?.Entry is { } nativeView)
				nativeView.MaxLength = searchBar.MaxLength;
		}

		public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler.PlatformView?.Entry is { } nativeView)
				nativeView.Attributes = nativeView.Attributes.AttrListFor(searchBar.CharacterSpacing);
		}

		[MissingMapper("GTK has no text prediction")]
		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar) { }

		public static void MapCancelButtonColor(IViewHandler handler, ISearchBar searchBar)
		{
			if (handler.PlatformView is Gtk.Widget platformView)
				platformView.SetStyleColor(searchBar.CancelButtonColor, "searchbar", "color", "entry > icon");
		}

		/// <summary>
		/// Maps the abstract <see cref="ITextInput.IsSpellCheckEnabled"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler"> The associated handler.</param>
		/// <param name="searchBar"> The associated <see cref="ISearchBar"/> instance.</param>
		[MissingMapper("spell checking follows the system input method")]
		public static void MapIsSpellCheckEnabled(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper("the keyboard is a soft-keyboard concept; GTK has no equivalent")]
		public static void MapKeyboard(IViewHandler handler, IView view) { }


	}

}