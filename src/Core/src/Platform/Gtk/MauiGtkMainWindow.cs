using System;
using System.Diagnostics;
using Gtk;
using Microsoft.Maui.LifecycleEvents;
using Application = GLib.Application;

namespace Microsoft.Maui
{

	public class MauiGtkMainWindow : Gtk.Window
	{

		public MauiGtkMainWindow() : base()
		{
			Shown += OnShown;
			Hidden += OnHidden;
			//GtkWidget::visibility-notify-event has been deprecated since version 3.12
			// VisibilityNotifyEvent += OnVisibilityNotifyEvent;
			CloseRequest += OnCloseRequest;

		}

		// GTK4: "delete-event" pasó a "close-request"; ya no se puede cancelar
		// modificando el evento, hay que devolver true/false.
		void OnCloseRequest(object? sender, Gtk.CloseRequestArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnDelete>(
				del => del(this, System.EventArgs.Empty));

			// GTK4 permite bloquear el cierre devolviendo true, pero este binding
			// expone close-request como void: aquí solo se notifica el cierre.
			if (MauiGtkApplication.Current.MainWindow == sender)
			{
				((Application)MauiGtkApplication.CurrentGtkApplication).Quit();
			}
		}

		//GtkWidget::visibility-notify-event has been deprecated since version 3.12
		// void OnVisibilityNotifyEvent(object o, VisibilityNotifyEventArgs args)
		// {
		// 	MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnVisibilityChanged>(del => del(this, args));
		// }

		void OnHidden(object? sender, EventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnHidden>(del => del(this, args));
		}

		void OnShown(object? sender, EventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnShown>(del => del(this, args));
		}

		void OnWindowStateEvent(object o, WindowStateEventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnStateChanged>(del => del(this, args));
		}

	}

}