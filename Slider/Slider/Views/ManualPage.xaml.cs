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
		//	BlueAction.Clicked += BlueAction_Clicked;
		//	SliderSlide.IsEnabled = false;
		//	SliderPan.IsEnabled = false;
		}

		private void SliderSlide_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LabelSlide.Text = ((int)SliderSlide.Value).ToString();
			var val = -Math.Round(SliderSlide.Value, 1);
			//	Debug.WriteLine($"++> Changed Value = {val}");
		//	Blue.Write($"sv{val};", Math.Abs(val) < 0.001);
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
		//	Blue.Write($"pv{val};", Math.Abs(val) < 0.001);
		}

		private void SliderPan_StoppedTracking(object sender, EventArgs e)
		{
			SliderPan.Value = 0;
		}
	}
}