using CamSlider.CustomControls;
using System;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSwitch), typeof(CamSlider.UWP.Renderers.FMSwitchRenderer))]
namespace CamSlider.UWP.Renderers
{
	public class FMSwitchRenderer : SwitchRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
				throw new Exception("Control is null!");
			Control.OnContent = (e.NewElement as FMSwitch).TextOn;
			Control.OffContent = (e.NewElement as FMSwitch).TextOff;
		}
	}
}
