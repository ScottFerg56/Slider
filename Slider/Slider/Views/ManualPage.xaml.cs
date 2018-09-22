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
			Comm.Pan.Zero();
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