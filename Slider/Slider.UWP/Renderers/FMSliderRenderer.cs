using System;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSlider), typeof(CamSlider.UWP.Renderers.FMSliderRenderer))]
namespace CamSlider.UWP.Renderers
{
	/// <summary>
	/// A subclass of the Xamarin SliderRenderer to support our FMSlider control subclass to expose a StoppedTracking event.
	/// </summary>
	public class FMSliderRenderer : SliderRenderer
	{
		/*
		 * The UWP Slider doesn't exactly have a StopTrackingTouch event for touchscreen interaction.
		 * But the PointerCaptureLost event comes close, so we just connect to that.
		 */

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Slider> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
				throw new Exception("Control is null!");

			// while we're here, let's turn off that annoying tooltip that tracks the thumb!
			Control.IsThumbToolTipEnabled = false;

			if (e.OldElement != null)
			{
				// Unsubscribe
				Control.PointerCaptureLost -= Slider_PointerCaptureLost;
			}
			if (e.NewElement != null)
			{
				// Subscribe
				Control.PointerCaptureLost += Slider_PointerCaptureLost;
			}
		}

		private void Slider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			// FINALLY, fire the StoppedTracking event on our custom FMSlider control!
			var slider = (CamSlider.CustomControls.FMSlider)Element;
			slider.OnStoppedTracking();
		}
	}
}
