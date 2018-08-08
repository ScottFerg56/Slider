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

			SliderComm.Instance.StateChange += Blue_StateChange;
			SliderSlide.ValueChanged += SliderSlide_ValueChanged;
			SliderSlide.StoppedTracking += SliderSlide_StoppedTracking;
			SliderPan.ValueChanged += SliderPan_ValueChanged;
			SliderPan.StoppedTracking += SliderPan_StoppedTracking;
			SliderSlide.IsEnabled = false;
			SliderPan.IsEnabled = false;
			Stepper.Slide.PropertyChanged += Slide_PropertyChanged;
			Stepper.Pan.PropertyChanged += Pan_PropertyChanged;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			// ANDROID HACK:
			// On Android these centered labels become left-aligned when the page disappears and reappears.
			// UGLY! But forcing a change seems to cause them to align properly.

			// the first time the Positions are requested will also go to the device
			// and bring us in sync with its current state
			LabelSlide.Text = "";
			LabelSlide.Text = Stepper.Slide.Position.ToString();
			LabelPan.Text = "";
			LabelPan.Text = Stepper.Pan.Position.ToString();
		}

		private void Slide_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					LabelSlide.Text = Stepper.Slide.Position.ToString();
				});
		}

		private void Pan_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position")
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					LabelPan.Text = Stepper.Pan.Position.ToString();
				});
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				SliderSlide.IsEnabled = SliderComm.Instance.State == BlueState.Connected;
				SliderPan.IsEnabled = SliderComm.Instance.State == BlueState.Connected;
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