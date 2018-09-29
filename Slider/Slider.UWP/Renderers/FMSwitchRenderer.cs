/*
OOOOOOO OO   OO  OOOOO             OO      O            OOO     OOOOOO                     OOO
 OO  OO OOO OOO OO   OO            OO     OO             OO      OO  OO                     OO
 OO   O OOOOOOO OO   OO                   OO             OO      OO  OO                     OO
 OO O   OOOOOOO  OO     OO   OO   OOO   OOOOOO   OOOOO   OO OO   OO  OO  OOOOO  OO OOO    OOOO   OOOOO  OO OOO   OOOOO  OO OOO
 OOOO   OO O OO   OOO   OO   OO    OO     OO    OO   OO  OOO OO  OOOOO  OO   OO  OOOOOO  OO OO  OO   OO  OO  OO OO   OO  OO  OO
 OO O   OO   OO     OO  OO O OO    OO     OO    OO       OO  OO  OO OO  OOOOOOO  OO  OO OO  OO  OOOOOOO  OO  OO OOOOOOO  OO  OO
 OO     OO   OO OO   OO OO O OO    OO     OO    OO       OO  OO  OO  OO OO       OO  OO OO  OO  OO       OO     OO       OO
 OO     OO   OO OO   OO  OOOOO     OO     OO OO OO   OO  OO  OO  OO  OO OO   OO  OO  OO OO  OO  OO   OO  OO     OO   OO  OO
OOOO    OO   OO  OOOOO    O O     OOOO     OOO   OOOOO  OOO  OO OOO  OO  OOOOO   OO  OO  OOO OO  OOOOO  OOOO     OOOOO  OOOO

	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

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
