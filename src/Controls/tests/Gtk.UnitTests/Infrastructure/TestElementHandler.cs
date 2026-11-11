using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Gtk.UnitTests.Infrastructure;

/// <summary>
/// Minimal handler used only to give a virtual view a <see cref="IMauiContext"/>.
/// The navigation tests drive <c>NavigationView</c> directly, so the
/// <c>NavigationPage</c> itself does not need a real platform view.
/// </summary>
public sealed class TestElementHandler : IViewHandler
{
	IView? _view;

	public IMauiContext? MauiContext { get; private set; }

	public object? PlatformView => null;

	public bool HasContainer { get; set; }

	public object? ContainerView => null;

	IView? IViewHandler.VirtualView => _view;

	IElement? IElementHandler.VirtualView => _view;

	public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

	public void SetVirtualView(IElement view) => _view = view as IView;

	public void UpdateValue(string property) { }

	public void Invoke(string command, object? args = null)
	{
		// Complete the navigation request like a real platform handler does;
		// otherwise NavigationPage waits forever for NavigationFinished.
		if (command == nameof(IStackNavigation.RequestNavigation) &&
			_view is IStackNavigation navigation &&
			args is NavigationRequest request)
		{
			navigation.NavigationFinished(request.NavigationStack);
		}
	}

	public void DisconnectHandler() => _view = null;

	public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

	public void PlatformArrange(Rect frame) { }
}
