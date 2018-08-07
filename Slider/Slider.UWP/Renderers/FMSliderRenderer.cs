using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSlider), typeof(CamSlider.UWP.Renderers.FMSliderRenderer))]
namespace CamSlider.UWP.Renderers
{
	public class FMSliderRenderer : SliderRenderer
	{
		Windows.UI.Xaml.Controls.Slider Slider;
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Slider> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
			{
				throw new Exception("Control is null!");
			//	Slider = new Windows.UI.Xaml.Controls.Slider();
			//	SetNativeControl(Slider);
			}
			else
			{
				Slider = Control;
			}

			Slider.IsThumbToolTipEnabled = false;

			if (e.OldElement != null)
			{
				// Unsubscribe
				Slider.PointerCaptureLost -= Slider_PointerCaptureLost;
			}
			if (e.NewElement != null)
			{
				// Subscribe
				Slider.PointerCaptureLost += Slider_PointerCaptureLost;
			}
		}

		private void Slider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			var slider = (CamSlider.CustomControls.FMSlider)Element;
			slider.OnStoppedTracking();
		}
	}
}
