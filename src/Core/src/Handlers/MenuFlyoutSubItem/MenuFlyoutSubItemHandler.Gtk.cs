using System;
using System.Collections.Generic;
using PlatformView = Microsoft.Maui.Platform.MauiMenuItem;

namespace Microsoft.Maui.Handlers
{
	public abstract class GtkMenuItemHandler<TVirtualView, TPlatformView, TVirtualItem, TPlatformItem> : ElementHandler<TVirtualView, TPlatformView>
		where TPlatformView : MauiMenuItem, new()
		where TVirtualView : class, IList<TVirtualItem>, IElement
		where TVirtualItem : IMenuElement
		where TPlatformItem : MauiMenuItem
	{
		protected GtkMenuItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

		protected override TPlatformView CreatePlatformElement()
		{
			return new();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in (TVirtualView)view)
			{
				Add(item);
			}
		}

		protected override void DisconnectHandler(TPlatformView platformView)
		{
			if (VirtualView is not null)
			{
				foreach (var item in VirtualView)
				{
					item.Handler?.DisconnectHandler();
				}
			}

			base.DisconnectHandler(platformView);
		}

		public void Add(TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.Handler!.PlatformView!;
			PlatformView.AppendSubItem(platformItem);
			platformItem.Show();
		}

		public void Remove(TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.Handler!.PlatformView!;
			PlatformView.RemoveSubItem(platformItem);
		}


		public void Insert(int index, TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.Handler!.PlatformView!;
			PlatformView.InsertSubItem(platformItem, index);
			platformItem.Show();
		}

		public void Clear()
		{
			PlatformView.ClearSubItems();
		}
	}

	public partial class MenuFlyoutSubItemHandler : GtkMenuItemHandler<IMenuFlyoutSubItem, PlatformView, IMenuElement, MauiMenuItem>
	{
		public static void MapText(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.UpdateText(view.Text);
		}

		/// <summary>
		///	Maps keyboard accelerator to the Windows platform control.
		/// </summary>
		/// <param name="handler">The handler, of type IMenuFlyoutSubItemHandler.</param>
		/// <param name="view">The view, of type IMenuFlyoutSubItem.</param>
		public static void MapKeyboardAccelerators(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.UpdateKeyboardAccelerators(view.KeyboardAccelerators);
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		public static void MapSource(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.UpdateImageSource(view.Source);
		}
	}
}