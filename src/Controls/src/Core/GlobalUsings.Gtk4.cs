// GTK4: la capa Compat del binding define Gtk.EventArgs y Gtk.EventHandler, que
// chocan con los de System en los archivos con `using Gtk;`.
#if GTK
global using EventArgs = System.EventArgs;
global using EventHandler = System.EventHandler;
#endif
