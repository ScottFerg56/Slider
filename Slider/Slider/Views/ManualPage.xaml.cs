using CamSlider.ViewModels;
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
		protected SliderComm Comm { get => SliderComm.Instance; }

		public ManualPage()
		{
			InitializeComponent();

			BindingContext = new ManualViewModel();

			SliderSlide.StoppedTracking += (s, e) => { SliderSlide.Value = 0; };
			SliderPan.StoppedTracking += (s, e) => { SliderPan.Value = 0; };
		}

		private void PanZero(object sender, EventArgs e)
		{
			Comm.Pan.Zero();
		}

		private void Calibrate(object sender, EventArgs e)
		{
			Comm.Slide.Home();
		}
	}
}