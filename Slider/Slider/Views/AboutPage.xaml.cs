using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CamSlider.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();

			SliderComm.Instance.StateChange += Blue_StateChange;
			BlueAction.Clicked += BlueAction_Clicked;
			SliderComm.Instance.Connect("SLIDER");
		}

		private void Blue_StateChange(object sender, EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				LabelBlueState.Text = SliderComm.Instance.StateText;
				var s = SliderComm.Instance.CanConnect ? "Connect" : "Disconnect";
				BlueAction.Text = s;
				this.Title = s;
				if (this.Parent is NavigationPage p)
					p.Title = s;
			});
		}

		private void BlueAction_Clicked(object sender, EventArgs e)
		{
			if (SliderComm.Instance.CanConnect)
				SliderComm.Instance.Connect("SLIDER");
			else
				SliderComm.Instance.Disconnect();
		}
	}
}