// GTK4: la capa Compat del binding define Gtk.EventArgs y Gtk.EventHandler, que
// chocan con los de System en todos los archivos que hacen `using Gtk;`.
// Se fijan los alias globales a los tipos de .NET (los eventos de MAUI).
global using EventArgs = System.EventArgs;
global using EventHandler = System.EventHandler;
