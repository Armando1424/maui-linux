#nullable disable
using TCollectionView = Microsoft.Maui.Controls.Handlers.Items.Platform.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override TCollectionView CreatePlatformView()
		{
			return new MauiCarouselView();
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdatePosition();
		}

		[MissingMapper("GTK's scrolled window does not expose the bounce/overshoot effect")]
		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateIsSwipeEnabled();
		}

		[MissingMapper("peek area insets are not implemented by the GTK layout managers")]
		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper("the GTK carousel does not wrap around yet")]
		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) { }

		public static void MapItemsLayout(CarouselViewHandler handler, CarouselView itemsView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateLayoutManager();
		}
	}
}