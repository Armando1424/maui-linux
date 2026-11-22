#nullable enable

// GTK4: Gtk.Clipboard desapareció. El equivalente es Gdk.Clipboard
// (Gdk.Display.Default.Clipboard) con API asíncrona (ReadTextAsync / ReadTextFinish).
// Para la prueba de compilación se deja un stub; implementarlo es trabajo aparte.
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ClipboardImplementation : IClipboard
	{
		public Task SetTextAsync(string? text)
		{
			// TODO(GTK4): Gdk.Display.Default.Clipboard.SetText(text)
			return Task.CompletedTask;
		}

		public bool HasText => false;

		public Task<string?> GetTextAsync()
		{
			// TODO(GTK4): ReadTextAsync + ReadTextFinish
			return Task.FromResult<string?>(null);
		}

		void StartClipboardListeners()
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		void StopClipboardListeners()
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}
	}
}
