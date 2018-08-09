using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ManualPage : ContentPage
	{

		public ManualPage()
		{
			InitializeComponent();

			SliderSlide.ValueChanged += SliderSlide_ValueChanged;
			SliderSlide.StoppedTracking += SliderSlide_StoppedTracking;
			SliderPan.ValueChanged += SliderPan_ValueChanged;
			SliderPan.StoppedTracking += SliderPan_StoppedTracking;
			this.IsEnabled = false;
			Stepper.Slide.PropertyChanged += Slide_PropertyChanged;
			Stepper.Pan.PropertyChanged += Pan_PropertyChanged;
			ButtonZero.Clicked += ButtonZero_Clicked;
		}

		private void ButtonZero_Clicked(object sender, EventArgs e)
		{
			Stepper.Pan.Zero();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			// ANDROID HACK:
			// On Android these centered labels become left-aligned when the page disappears and reappears.
			// UGLY! But forcing a change seems to cause them to align properly.
			LabelSlide.Text = "";
			LabelSlide.Text = Stepper.Slide.Position.ToString();
			LabelPan.Text = "";
			LabelPan.Text = Stepper.Pan.Position.ToString();
		}

		private void Slide_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					LabelSlide.Text = Stepper.Slide.Position.ToString();
				});
			}
			else if (e.PropertyName == "Homed")
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					this.IsEnabled = Stepper.Slide.Homed;
				});
			}
		}

		private void Pan_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					LabelPan.Text = Stepper.Pan.Position.ToString();
				});
		}

		private void SliderSlide_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			//	Debug.WriteLine($"++> Changed Value = {val}");
			SliderComm.Instance.SetSlideVector(-SliderSlide.Value);
		}

		private void SliderSlide_StoppedTracking(object sender, EventArgs e)
		{
			//	Debug.WriteLine($"++> StoppedTracking Value = {-Math.Round(SliderSlide.Value, 1)}");
			SliderSlide.Value = 0;
		}

		private void SliderPan_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			SliderComm.Instance.SetPanVector(-SliderPan.Value);
		}

		private void SliderPan_StoppedTracking(object sender, EventArgs e)
		{
			SliderPan.Value = 0;
		}
	}
}