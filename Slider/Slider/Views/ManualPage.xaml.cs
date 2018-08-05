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
		BlueApp Blue;

		public ManualPage()
		{
			InitializeComponent();

			Blue = new BlueApp();
			Blue.StateChange += Blue_StateChange;

			SliderSlide.ValueChanged += SliderSlide_ValueChanged;
			SliderSlide.StoppedTracking += SliderSlide_StoppedTracking;
			SliderPan.ValueChanged += SliderPan_ValueChanged;
			SliderPan.StoppedTracking += SliderPan_StoppedTracking;
			BlueAction.Clicked += BlueAction_Clicked;
			SliderSlide.IsEnabled = false;
			SliderPan.IsEnabled = false;
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				string state = Blue.State.ToString();
				if (!string.IsNullOrWhiteSpace(Blue.ErrorMessage))
					state += " - " + Blue.ErrorMessage;
				LabelBlueState.Text = state;
				BlueAction.Text = Blue.CanConnect ? "Connect" : "Disconnect";
				SliderSlide.IsEnabled = Blue.State == BlueState.Connected;
				SliderPan.IsEnabled = Blue.State == BlueState.Connected;
			});
		}

		private void BlueAction_Clicked(object sender, EventArgs e)
		{
			if (Blue.CanConnect)
				Blue.Connect("SLIDER");
			else
				Blue.Disconnect();
		}

		private void SliderSlide_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LabelSlide.Text = ((int)SliderSlide.Value).ToString();
			var val = -Math.Round(SliderSlide.Value, 1);
			//	Debug.WriteLine($"++> Changed Value = {val}");
			Blue.Write($"sv{val};", Math.Abs(val) < 0.001);
		}

		private void SliderSlide_StoppedTracking(object sender, EventArgs e)
		{
			//	Debug.WriteLine($"++> StoppedTracking Value = {-Math.Round(SliderSlide.Value, 1)}");
			SliderSlide.Value = 0;
		}

		private void SliderPan_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LabelPan.Text = ((int)SliderPan.Value).ToString();
			var val = -Math.Round(SliderPan.Value, 1);
			Blue.Write($"pv{val};", Math.Abs(val) < 0.001);
		}

		private void SliderPan_StoppedTracking(object sender, EventArgs e)
		{
			SliderPan.Value = 0;
		}
	}
}