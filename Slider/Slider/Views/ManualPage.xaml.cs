/*
OO   OO                                   OOO   OOOOOO
OOO OOO                                    OO    OO  OO
OOOOOOO                                    OO    OO  OO
OOOOOOO  OOOO   OO OOO  OO  OO   OOOO      OO    OO  OO  OOOO    OOO OO  OOOOO
OO O OO     OO   OOOOOO OO  OO      OO     OO    OOOOO      OO  OO  OO  OO   OO
OO   OO  OOOOO   OO  OO OO  OO   OOOOO     OO    OO      OOOOO  OO  OO  OOOOOOO
OO   OO OO  OO   OO  OO OO  OO  OO  OO     OO    OO     OO  OO  OO  OO  OO
OO   OO OO  OO   OO  OO OO  OO  OO  OO     OO    OO     OO  OO   OOOOO  OO   OO
OO   OO  OOO OO  OO  OO  OOO OO  OOO OO   OOOO  OOOO     OOO OO     OO   OOOOO
                                                                OO  OO
                                                                 OOOO
	(c) 2018 Scott Ferguson
	This code is licensed under MIT license(see LICENSE file for details)
*/

using CamSlider.ViewModels;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ManualPage : ContentPage
	{
		protected SliderComm Comm { get => SliderComm.Instance; }

		public ManualPage()
		{
			InitializeComponent();

			BindingContext = new ManualViewModel();

			// when the sliders for moving the Slide and Pan steppers are released,
			// we want them to pop back to center/0 so movement will stop
			SliderSlide.StoppedTracking += (s, e) => { SliderSlide.Value = 0; };
			SliderPan.StoppedTracking += (s, e) => { SliderPan.Value = 0; };
		}

		/// <summary>
		/// Respond to the PanZero button click by setting the current Pan position to 0.
		/// </summary>
		private void PanZero(object sender, EventArgs e)
		{
			Comm.Pan.Position = 0;
		}

		/// <summary>
		/// Respond to the Slide Calibrate button click by initiating a Homing action
		/// that will calibrate the Slide by sending it to the limit switch.
		/// </summary>
		private void Calibrate(object sender, EventArgs e)
		{
			Comm.Slide.Calibrated = false;
		}
	}
}