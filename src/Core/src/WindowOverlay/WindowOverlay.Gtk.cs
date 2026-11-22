namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
#pragma warning disable CS0649 // el overlay GTK aún no conecta la vista gráfica
		Gtk.Widget? _graphicsView;
#pragma warning restore CS0649

		/// <inheritdoc/>
		public void Invalidate()
		{
		}

		/// <inheritdoc/>
		public virtual bool Initialize()
		{
			return IsPlatformViewInitialized = true;
		}

		/// <summary>
		/// Deinitializes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		void DeinitializePlatformDependencies()
		{
			IsPlatformViewInitialized = false;
		}
	}
}