using System;
using Cairo;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{

	/// <summary>
	/// dummy widget to mark handler as not implemented
	/// but avoid <see cref="NotImplementedException"/>
	/// </summary>
	public class NotImplementedView : Gtk.EventBox
	{

		protected NotImplementedView() { }

		public NotImplementedView(string name) : this()
		{
			DisplayName = name;

			// GTK4: sin acceso directo a un Graphene.Rect para AppendCairo,
			// se marca el widget con una etiqueta simple.
			Child = new Gtk.Label(name) { Xalign = 0 };

		}

		public string? DisplayName { get; }




	}

}