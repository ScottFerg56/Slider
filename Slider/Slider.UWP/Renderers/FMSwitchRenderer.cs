//	(c) 2018 Scott Ferguson
//	This code is licensed under MIT license(see LICENSE file for details)

using CamSlider.CustomControls;
using System;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CamSlider.CustomControls.FMSwitch), typeof(CamSlider.UWP.Renderers.FMSwitchRenderer))]
namespace CamSlider.UWP.Renderers
{
	/// <summary>
	/// A subclass of the Xamarin SliderRenderer to support our FMSwitch control subclass to expose expose custom labels for ON and OFF.
	/// </summary>
	public class FMSwitchRenderer : SwitchRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
		{
			base.OnElementChanged(e);
			if (Control == null)
				throw new Exception("Control is null!");

			// set the labels on the platform's control
			Control.OnContent = (e.NewElement as FMSwitch).TextOn;
			Control.OffContent = (e.NewElement as FMSwitch).TextOff;
		}
	}
}
