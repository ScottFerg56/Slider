using System;
using System.Diagnostics;
using System.Linq;
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
			//	this.Title = s;
			//	if (this.Parent is NavigationPage p)
			//		p.Title = s;
				switch (SliderComm.Instance.State)
				{
					case BlueState.Disconnected:
						Slider.Services.PlaySound.Play("down");
						break;
					case BlueState.Connected:
						Slider.Services.PlaySound.Play("up");
						// switch to the ManualPage once we're connected
						if (Parent.Parent is TabbedPage t)
						{
							// ANDROID ISSUE:
							// There's apparently some kind of timing problem such that the ManualPage
							// has already appeared (before connection) so it gets no OnAppearing notification
							// from this navigation (after conection)

							// our Parent is a NavigationPage and his Parent is the TabbedPage (MainPage)
							// we'll find the ManualPage among the TabbedPage's grandchildren
						//	t.CurrentPage = t.Children.First(c => ((NavigationPage)c).CurrentPage is ManualPage);
						}
						break;
				}
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