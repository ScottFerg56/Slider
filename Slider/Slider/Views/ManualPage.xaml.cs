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
			LabelSlide.Text = ((int)SliderSlide.Value).ToString();
			SliderComm.Instance.SetSlideVector(-SliderSlide.Value);
		}

		private void SliderSlide_StoppedTracking(object sender, EventArgs e)
		{
			//	Debug.WriteLine($"++> StoppedTracking Value = {-Math.Round(SliderSlide.Value, 1)}");
			SliderSlide.Value = 0;
		}

		private void SliderPan_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LabelPan.Text = ((int)SliderPan.Value).ToString();
			SliderComm.Instance.SetPanVector(-SliderPan.Value);
		}

		private void SliderPan_StoppedTracking(object sender, EventArgs e)
		{
			SliderPan.Value = 0;
		}
	}
}